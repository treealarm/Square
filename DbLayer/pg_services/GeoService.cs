using Domain;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class GeoService : IGeoService
  {
    private readonly ILevelService _levelService;
    private readonly PgDbContext _dbContext;
    public GeoService(
      PgDbContext dbContext,
      ILevelService levelService
    )
    {
      _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
      _levelService = levelService ?? throw new ArgumentNullException(nameof(levelService));
    }

    public async Task<Dictionary<string, GeoObjectDTO>> GetGeoAsync(BoxDTO box)
    {
      int limit = box.count.HasValue && box.count.Value > 0 ? box.count.Value : 10000;
      IQueryable<DBGeoObject> query = _dbContext.GeoObjects;
      var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

      // Гео-фильтр
      if (box.zone != null && box.zone.Count > 0)
      {
        var polygons = box.zone
            .Select(z => ModelGate.ConvertGeoDTO2DB(z))
            .Where(g => g != null)
            .ToList();

        query = box.not_in_zone
            ? query.Where(t => polygons.All(p => !t.figure.Intersects(p)))
            : query.Where(t => polygons.Any(p => t.figure.Intersects(p)));
      }
      else
      {
        var rect = geometryFactory.CreatePolygon(new[]
        {
            new Coordinate(box.wn[0], box.wn[1]),
            new Coordinate(box.es[0], box.wn[1]),
            new Coordinate(box.es[0], box.es[1]),
            new Coordinate(box.wn[0], box.es[1]),
            new Coordinate(box.wn[0], box.wn[1])
        });

        query = box.not_in_zone
            ? query.Where(t => !t.figure.Intersects(rect))
            : query.Where(t => t.figure.Intersects(rect));
      }

      // Фильтр по zoom_level
      if (box.zoom != null)
      {
        var levels = await _levelService.GetLevelsByZoom(box.zoom);
        levels.Add(null);
        levels.Add(string.Empty);
        query = query.Where(p => levels.Contains(p.zoom_level));
      }

      // Фильтр по id (строки -> Guid)
      if (box.ids != null && box.ids.Count > 0)
      {
        var guidIds = box.ids
            .Select(Domain.Utils.ConvertObjectIdToGuid)
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

        query = query.Where(p => guidIds.Contains(p.id));
      }

      // Выполнение запроса
      var list = await query.Take(limit).ToListAsync();
      return ModelGate.ConvertListDB2DTO(list);
    }


    public async Task<Dictionary<string, GeoObjectDTO>> GetGeoObjectsAsync(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return new Dictionary<string, GeoObjectDTO>();

      // Конвертируем строки в Guid
      var guidIds = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (guidIds.Count == 0)
        return new Dictionary<string, GeoObjectDTO>();

      List<DBGeoObject> obj = null;

      try
      {
        obj = await _dbContext.GeoObjects
            .Where(x => guidIds.Contains(x.id))
            .ToListAsync();
      }
      catch (Exception)
      {
        // Логируем или игнорируем
      }

      return ModelGate.ConvertListDB2DTO(obj);
    }


    public async Task<GeoObjectDTO> GetGeoObjectAsync(string id)
    {
      if (string.IsNullOrEmpty(id))
        return null;

      Guid? guidId = Domain.Utils.ConvertObjectIdToGuid(id);
      if (!guidId.HasValue)
        return null;

      DBGeoObject obj = null;

      try
      {
        obj = await _dbContext.GeoObjects
            .FirstOrDefaultAsync(x => x.id == guidId.Value);
      }
      catch (Exception)
      {
        // Можно логировать ошибку
      }

      return ModelGate.ConvertDB2DTO(obj);
    }


    public async Task<long> RemoveAsync(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return 0;

      var guidIds = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (guidIds.Count == 0)
        return 0;

      var entities = await _dbContext.GeoObjects
          .Where(x => guidIds.Contains(x.id))
          .ToListAsync();

      if (entities.Count == 0)
        return 0;

      _dbContext.GeoObjects.RemoveRange(entities);
      await _dbContext.SaveChangesAsync();

      return entities.Count;
    }


    public async Task<Dictionary<string, GeoObjectDTO>> CreateGeo(IEnumerable<FigureGeoDTO> figures)
    {
      var retVal = new Dictionary<string, GeoObjectDTO>();
      if (figures == null || !figures.Any())
        return retVal;

      var dbUpdated = new Dictionary<FigureGeoDTO, DBGeoObject>();

      // Конвертация DTO → DB
      var dbObjects = figures.Select(f =>
      {
        var dbObj = new DBGeoObject
        {
          zoom_level = f.zoom_level,
          radius = f.radius,
          figure = ModelGate.ConvertGeoDTO2DB(f.geometry),
          id = Domain.Utils.ConvertObjectIdToGuid(f.id) ?? Guid.Empty
        };
        dbUpdated[f] = dbObj;
        return dbObj;
      }).ToList();

      // Список существующих id (не пустых)
      var ids = dbObjects.Where(o => o.id != Guid.Empty).Select(o => o.id).ToList();

      // Загружаем существующие объекты из базы
      var existing = await _dbContext.GeoObjects
          .Where(g => ids.Contains(g.id))
          .ToListAsync();
      var existingDict = existing.ToDictionary(e => e.id);

      foreach (var obj in dbObjects)
      {
        if (obj.id == Guid.Empty || !existingDict.ContainsKey(obj.id))
        {
          // Новая запись → генерируем Guid, если пустой
          if (obj.id == Guid.Empty)
            obj.id = Domain.Utils.NewGuid();

          _dbContext.GeoObjects.Add(obj);
        }
        else
        {
          // Существующая запись → обновляем поля
          var existingObj = existingDict[obj.id];
          existingObj.figure = obj.figure;
          existingObj.radius = obj.radius;
          existingObj.zoom_level = obj.zoom_level;
        }
      }

      // Сохраняем изменения
      await _dbContext.SaveChangesAsync();

      // Проставляем id обратно в DTO и формируем результат
      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id.ToString();
        retVal[pair.Value.id.ToString()] = ModelGate.ConvertDB2DTO(pair.Value);
      }

      return retVal;
    }
  }
}
