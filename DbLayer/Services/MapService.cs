using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class MapService : IMapService
  {
    private readonly PgDbContext _db;

    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDB;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDBSettings;   

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings, 
        IMongoClient mongoClient, 
        PgDbContext db)
    {
      _geoStoreDBSettings = geoStoreDatabaseSettings;
      _mongoClient = mongoClient;

      _mongoDB = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _db = db;

      CreateIndexes();
    }

    private void CreateIndexes()
    {

    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetAsync(List<string> ids)
    {
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g != null)
          .Select(g => g.Value)
          .ToList();

      if (guids.Count == 0)
        return new Dictionary<string, BaseMarkerDTO>();

      var entities = await _db.Markers
          .Where(m => guids.Contains(m.id))
          .ToListAsync();

      var result = new Dictionary<string, BaseMarkerDTO>();

      foreach (var entity in entities)
      {
        var dto = new MarkerDTO();
        entity.CopyAllTo(dto);

        // для совместимости возвращаем ключ словаря как ObjectId-строку
        var key = Domain.Utils.ConvertGuidToObjectId(entity.id);
        result[key] = dto;
      }

      return result;
    }


    public async Task<BaseMarkerDTO> GetAsync(string id)
    {
      var guid = Domain.Utils.ConvertObjectIdToGuid(id);
      if (guid == null)
        return null;

      var entity = await _db.Markers.FirstOrDefaultAsync(m => m.id == guid.Value);
      if (entity == null)
        return null;

      var dto = new MarkerDTO();
      entity.CopyAllTo(dto); // твой extension для копирования
      return dto;
    }

    public async Task<BaseMarkerDTO> GetParent(string id)
    {
      var guid = Domain.Utils.ConvertObjectIdToGuid(id);
      if (guid == null)
        return null;

      var entity = await _db.Markers
          .Where(m => m.id == guid.Value)
          .Select(m => m.parent_id)
          .FirstOrDefaultAsync();

      if (entity == Guid.Empty)
        return null;

      var parent = await _db.Markers.FirstOrDefaultAsync(m => m.id == entity);
      if (parent == null)
        return null;

      var dto = new MarkerDTO();
      parent.CopyAllTo(dto);
      return dto;
    }


    public async Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdsAsync(
        List<string> parent_ids,
        string start_id,
        string end_id,
        int count)
    {
      // Сначала конвертируем parent_ids в Guid, игнорируя пустые строки
      var parentGuids = parent_ids
          .Where(id => !string.IsNullOrEmpty(id))
          .Select(id => Domain.Utils.ConvertObjectIdToGuid(id))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      // Флаг, есть ли среди parent_ids пустые или null
      bool includeNull = parent_ids.Any(string.IsNullOrEmpty);


      if (!parentGuids.Any() && !includeNull)
        return new Dictionary<string, BaseMarkerDTO>();

      IQueryable<DBMarker> query = _db.Markers
          .Where(m => (m.parent_id.HasValue && parentGuids.Contains(m.parent_id.Value))
                      || (!m.parent_id.HasValue && includeNull))
          .OrderBy(m => m.id);


      // Применяем фильтр по start_id / end_id
      if (!string.IsNullOrEmpty(start_id))
      {
        var startGuid = Domain.Utils.ConvertObjectIdToGuid(start_id);
        if (startGuid != null)
        {
          query = query.Where(m => m.id.CompareTo(startGuid.Value) >= 0);            
        }
      }
      else if (!string.IsNullOrEmpty(end_id))
      {
        var endGuid = Domain.Utils.ConvertObjectIdToGuid(end_id);
        if (endGuid != null)
        {
          query = query.Where(m => m.id.CompareTo(endGuid.Value) <= 0)
                       .OrderByDescending(m => m.id);
        }
      }

      var list = await query.Take(count).ToListAsync();

      // Если сортировали по end_id, то вернём по возрастанию
      if (!string.IsNullOrEmpty(end_id))
        list.Sort((x, y) => x.id.CompareTo(y.id));

      return list.ToDictionary(m => Domain.Utils.ConvertGuidToObjectId(m.id), m =>
      {
        var dto = new BaseMarkerDTO();
        m.CopyAllTo(dto);
        return dto;
      });
    }


    public async Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdAsync(
      string parent_id,
      string start_id,
      string end_id,
      int count
    )
    {
      return await GetByParentIdsAsync(new List<string>() { parent_id }, start_id, end_id, count);
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetByNameAsync(string name)
    {
      var list = await _db.Markers
          .Where(m => m.name == name)
          .ToListAsync();

      return list.ToDictionary(
          m => Domain.Utils.ConvertGuidToObjectId(m.id), // ключ — старый ObjectId строкой
          m =>
          {
            var dto = new BaseMarkerDTO();
            m.CopyAllTo(dto);
            return dto;
          });
    }


    public async Task<List<BaseMarkerDTO>> GetByChildIdAsync(string object_id)
    {
      var parents = new List<BaseMarkerDTO>();

      var marker = await GetAsync(object_id); // используем уже адаптированный GetAsync(string id)
      while (marker != null)
      {
        parents.Add(marker);

        if (string.IsNullOrEmpty(marker.parent_id))
        {
          break; // достигли корня
        }

        var parentGuid = Domain.Utils.ConvertObjectIdToGuid(marker.parent_id);
        if (parentGuid == null)
          break;

        var parentEntity = await _db.Markers
            .FirstOrDefaultAsync(m => m.id == parentGuid.Value);

        if (parentEntity == null)
          break;

        marker = new BaseMarkerDTO
        {
          parent_id = parentEntity.parent_id.HasValue ? Domain.Utils.ConvertGuidToObjectId(parentEntity.parent_id.Value) : null,
          owner_id = parentEntity.owner_id.HasValue ? Domain.Utils.ConvertGuidToObjectId(parentEntity.owner_id.Value) : null,
          name = parentEntity.name,
          id = Domain.Utils.ConvertGuidToObjectId(parentEntity.id)
        };
      }

      return parents;
    }


    public async Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id)
    {
      var result = new List<BaseMarkerDTO>();
      var cb = new ConcurrentBag<List<BaseMarkerDTO>>();

      var children = await GetByParentIdAsync(parent_id, null, null, int.MaxValue);

      cb.Add(children.Values.ToList());

      foreach (var item in children)
      {
        var sub_children = await GetAllChildren(item.Value.id);
        cb.Add(sub_children);
      }

      foreach (var list in cb)
      {
        result.AddRange(list);
      }

      return result;
    }

    BaseMarkerDTO ConvertMarkerDB2DTO(DBMarker dbMarker)
    {
      if (dbMarker == null)
      {
        return null;
      }

      BaseMarkerDTO result = new BaseMarkerDTO();
      dbMarker.CopyAllTo(result);
      return result;
    }
    Dictionary<string, BaseMarkerDTO> ConvertMarkerListDB2DTO(List<DBMarker> dbMarkers)
    {
      var result = new Dictionary<string, BaseMarkerDTO>();

      foreach (var dbItem in dbMarkers)
      {
        result.Add(Domain.Utils.ConvertGuidToObjectId(dbItem.id), ConvertMarkerDB2DTO(dbItem));
      }

      return result;
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetTopChildren(List<string> parentIds)
    {
      // Конвертируем parentIds в Guid, игнорируя пустые или некорректные
      var parentGuids = parentIds
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!parentGuids.Any())
        return new Dictionary<string, BaseMarkerDTO>();

      // Берём "первого" ребёнка для каждого parent_id
      var topChildren = await _db.Markers
          .Where(m => m.parent_id.HasValue && parentGuids.Contains(m.parent_id.Value))
          .GroupBy(m => m.parent_id.Value)
          .Select(g => g.OrderBy(m => m.id).FirstOrDefault())
          .ToListAsync();

      var dict = new Dictionary<string, BaseMarkerDTO>();
      foreach (var entity in topChildren)
      {
        if (entity != null)
        {
          var dto = new BaseMarkerDTO
          {
            id = Domain.Utils.ConvertGuidToObjectId(entity.id),
          };
          dict[Domain.Utils.ConvertGuidToObjectId(entity.parent_id??Guid.Empty)] = dto;
        }
      }

      return dict;
    }

    public async Task UpdateHierarchyAsync(IEnumerable<BaseMarkerDTO> updatedList)
    {
      if (!updatedList.Any())
        return;

      var ids = updatedList
        .Select(u => Domain.Utils.ConvertObjectIdToGuid(u.id))
        .Where(g => g.HasValue)
        .Select(g => g.Value)
        .ToList();

      var existingEntities = await _db.Markers
          .Where(m => ids.Contains(m.id))
          .ToDictionaryAsync(m => m.id);

      var dbUpdated = new Dictionary<BaseMarkerDTO, DBMarker>();

      foreach (var item in updatedList)
      {
        var dbObj = new DBMarker();
        item.CopyAllTo(dbObj);

        // parent_id и owner_id как Guid? (nullable)
        dbObj.parent_id = string.IsNullOrEmpty(item.parent_id)
            ? null
            : Domain.Utils.ConvertObjectIdToGuid(item.parent_id);

        dbObj.owner_id = string.IsNullOrEmpty(item.owner_id)
            ? null
            : Domain.Utils.ConvertObjectIdToGuid(item.owner_id);

        // id
        if (string.IsNullOrEmpty(item.id))
        {
          var newGuid = Guid.NewGuid();
          dbObj.id = Domain.Utils.ConvertObjectIdToGuid(
              Domain.Utils.ConvertGuidToObjectId(newGuid)) ?? newGuid;
        }

        dbUpdated.Add(item, dbObj);
      }

      // Сохраняем все объекты через EF Core
      foreach (var kvp in dbUpdated)
      {
        if (!existingEntities.TryGetValue(kvp.Value.id, out var entity))
        {
          _db.Markers.Add(kvp.Value);
        }
        else
        {
          _db.Markers.Update(entity);
        }          
      }

      await _db.SaveChangesAsync();

      // Обновляем DTO
      foreach (var pair in dbUpdated)
      {
        pair.Key.id = Domain.Utils.ConvertGuidToObjectId(pair.Value.id);
      }
    }


    public async Task<long> RemoveAsync(List<string> ids)
    {
      List<ObjectId> objIds = ids.Select(s => new ObjectId(s)).ToList();
      // Конвертируем ObjectId строки в Guid
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!guids.Any())
        return 0;

      var filter = Builders<BsonDocument>.Filter.In("meta.figure._id", objIds);

      var tracks = _mongoDB.GetCollection<BsonDocument>(
          _geoStoreDBSettings.Value.TracksCollectionName);

      var res1 = await tracks.DeleteManyAsync(filter);

      var routes = _mongoDB.GetCollection<BsonDocument>(
          _geoStoreDBSettings.Value.RoutesCollectionName);

      await routes.DeleteManyAsync(filter);

      var filter1 = Builders<BsonDocument>.Filter.In("_id", objIds);

      var states = _mongoDB.GetCollection<BsonDocument>(
      _geoStoreDBSettings.Value.StateCollectionName);

      await states.DeleteManyAsync(filter1);

      var propsToDelete = await _db.Properties
         .Where(x => guids.Contains(x.id))
         .ToListAsync();

      _db.Properties.RemoveRange(propsToDelete);

      var deletedCount = await _db.Markers
        .Where(m => guids.Contains(m.id))
        .ExecuteDeleteAsync();

      return deletedCount;
    }

    private static DBMarkerProperties ConvertDTO2Property(IObjectProps propsIn)
    {
      var props = propsIn as IObjectProps;

      DBMarkerProperties mProps = new DBMarkerProperties()
      {
        extra_props = new List<MarkerProp>(),
        id = Domain.Utils.ConvertObjectIdToGuid(propsIn.id) ?? Guid.Empty
      };

      if (props.extra_props == null)
      {
        return mProps;
      }

      mProps.extra_props = ModelGate.ConvertExtraPropsToDB<MarkerProp>(props.extra_props);

      return mProps;
    }

    async Task IMapService.UpdatePropAsync(IEnumerable<ObjPropsDTO> updatedObjs)
    {
      if (!updatedObjs.Any())
        return;

      var idsToUpdate = updatedObjs
          .Select(x => Domain.Utils.ConvertObjectIdToGuid(x.id))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      var existing = await _db.Properties
          .Include(x => x.extra_props)
          .Where(x => idsToUpdate.Contains(x.id))
          .ToListAsync();

      var existingDict = existing.ToDictionary(x => x.id);

      var toUpdate = new List<DBMarkerProperties>();
      var toAdd = new List<DBMarkerProperties>();

      foreach (var dto in updatedObjs)
      {
        var objId = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Guid.Empty;

        var props = dto.extra_props.Select(p => new MarkerProp
        {
          prop_name = p.prop_name,
          str_val = p.str_val,
          visual_type = p.visual_type,
          owner_id = objId
        }).ToList();

        if (existingDict.TryGetValue(objId, out var existingEntity))
        {
          // Очищаем старые свойства и заменяем новыми
          existingEntity.extra_props.Clear();
          existingEntity.extra_props.AddRange(props);

          toUpdate.Add(existingEntity);
        }
        else
        {
          var newEntity = new DBMarkerProperties
          {
            id = objId,
            extra_props = props
          };

          toAdd.Add(newEntity);
        }
      }

      _db.Properties.UpdateRange(toUpdate);
      await _db.Properties.AddRangeAsync(toAdd);
      await _db.SaveChangesAsync();
    }


    public async Task UpdatePropNotDeleteAsync(IEnumerable<IObjectProps> listUpdate)
    {
      if (!listUpdate.Any())
        return;

      var idsToUpdate = listUpdate.Select(x => Domain.Utils.ConvertObjectIdToGuid(x.id) ?? Guid.Empty)
                                  .ToList();

      var existingEntities = await _db.Properties
          .Include(p => p.extra_props)
          .Where(p => idsToUpdate.Contains(p.id))
          .ToListAsync();

      var existingDict = existingEntities.ToDictionary(e => e.id);

      foreach (var dto in listUpdate)
      {
        if (dto?.extra_props?.Any() != true)
          continue;

        var objId = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Guid.Empty;

        var newProps = dto.extra_props.Select(p => new MarkerProp
        {
          prop_name = p.prop_name,
          str_val = p.str_val,
          visual_type = p.visual_type,
          owner_id = objId
        }).ToList();

        if (existingDict.TryGetValue(objId, out var existingEntity))
        {
          // Удаляем из коллекции только свойства с совпадающими prop_name
          existingEntity.extra_props.RemoveAll(ep => newProps.Select(np => np.prop_name).Contains(ep.prop_name));

          // Добавляем новые/обновлённые
          existingEntity.extra_props.AddRange(newProps);

          _db.Properties.Update(existingEntity);
        }
        else
        {
          // Если объекта нет, создаём новый
          var newEntity = new DBMarkerProperties
          {
            id = objId,
            extra_props = newProps
          };
          await _db.Properties.AddAsync(newEntity);
        }
      }

      await _db.SaveChangesAsync();
    }



    public async Task<ObjPropsDTO> GetPropAsync(string id)
    {
      // Преобразуем id в Guid (если в PostgreSQL используем Guid)
      var guidId = Domain.Utils.ConvertObjectIdToGuid(id) ?? Guid.Empty;

      var entity = await _db.Properties
          .Include(p => p.extra_props) // включаем дочерние свойства
          .FirstOrDefaultAsync(p => p.id == guidId);

      return ModelGate.Conver2Property2DTO<DBMarkerProperties, MarkerProp>(entity);
    }


    public async Task<Dictionary<string, ObjPropsDTO>> GetPropsAsync(List<string> ids)
    {
      var guidIds = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      var entities = await _db.Properties
          .Include(p => p.extra_props)      // загружаем дочерние свойства
          .Where(p => guidIds.Contains(p.id))
          .ToListAsync();

      var result = entities.ToDictionary(
          e => Domain.Utils.ConvertGuidToObjectId(e.id),             // преобразуем обратно в string, если DTO использует string id
          e => ModelGate.Conver2Property2DTO<DBMarkerProperties, MarkerProp>(e)
      );

      return result;
    }


    public async Task<List<ObjPropsDTO>> GetPropByValuesAsync(
        ObjPropsSearchDTO propFilter,
        string start_id,
        int forward,
        int count)
    {
      var query = _db.Properties
          .Include(p => p.extra_props)
          .AsQueryable();

      // Пагинация по Guid
      if (!string.IsNullOrEmpty(start_id))
      {
        var startGuid = Domain.Utils.ConvertObjectIdToGuid(start_id) ?? Guid.Empty;

        if (forward > 0)
          query = query.Where(p => p.id.CompareTo(startGuid) > 0);
        else
          query = query.Where(p => p.id.CompareTo(startGuid) < 0);
      }

      // Фильтр по свойствам
      if (propFilter?.props?.Any() == true)
      {
        var propNames = propFilter.props.Select(p => p.prop_name).ToList();
        var propValues = propFilter.props.ToDictionary(p => p.prop_name, p => p.str_val);

        query = query.Where(p =>
            p.extra_props.Any(ep =>
                propValues.ContainsKey(ep.prop_name) &&
                ep.str_val == propValues[ep.prop_name]
            )
        );
      }

      // Сортировка
      query = forward > 0 ? query.OrderBy(p => p.id) : query.OrderByDescending(p => p.id);

      // Лимит
      var entities = await query.Take(count).ToListAsync();

      // Если сортировка была Desc и forward <= 0, нужно вернуть в Asc (как у тебя)
      if (forward <= 0)
        entities.Reverse();

      return entities.Select(ModelGate.Conver2Property2DTO<DBMarkerProperties, MarkerProp>).ToList();
    }


    public async Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAsync(List<string> ids)
    {
      // Конвертируем ObjectId строки в Guid
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!guids.Any())
        return new Dictionary<string, BaseMarkerDTO>();

      // Получаем все объекты с этими id
      var listObjects = await _db.Markers
          .Where(m => guids.Contains(m.id))
          .ToListAsync();

      // Представления — объекты с owner_id != null
      var views = listObjects
          .Where(m => m.owner_id.HasValue)
          .ToList();

      if (!views.Any())
      {
        // Все объекты без owner_id
        return ConvertMarkerListDB2DTO(listObjects);
      }

      // Владельцы — объекты с owner_id == null
      var owners = listObjects
          .Where(m => !m.owner_id.HasValue)
          .ToList();

      var ownerGuids = views
          .Select(v => v.owner_id.Value)
          .ToList();

      // Получаем владельцев из базы, которых еще нет в listObjects
      var ownersDb = await _db.Markers
          .Where(m => !m.owner_id.HasValue && ownerGuids.Contains(m.id))
          .ToListAsync();

      owners = owners.Union(ownersDb).ToList();

      return ConvertMarkerListDB2DTO(owners);
    }


    public async Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAndViewsAsync(List<string> ids)
    {
      // Конвертируем строки в Guid
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!guids.Any())
        return new Dictionary<string, BaseMarkerDTO>();

      // Получаем все маркеры с указанными ID
      var listObjects = await _db.Markers
          .Where(m => guids.Contains(m.id))
          .ToListAsync();

      // Представления — объекты с owner_id != null
      var views = listObjects
          .Where(m => m.owner_id.HasValue)
          .ToList();

      if (!views.Any())
      {
        return ConvertMarkerListDB2DTO(listObjects);
      }

      // Владельцы среди уже полученных объектов (owner_id == null)
      var owners = listObjects
          .Where(m => !m.owner_id.HasValue)
          .ToList();

      // Список owner_id для поиска недостающих владельцев
      var ownerGuids = views
          .Select(v => v.owner_id.Value)
          .ToList();

      // Получаем владельцев из базы, которых ещё нет в owners
      var ownersDb = ownerGuids.Any()
          ? await _db.Markers
              .Where(m => !m.owner_id.HasValue && ownerGuids.Contains(m.id))
              .ToListAsync()
          : new List<DBMarker>();

      // Объединяем представления и владельцев
      var allObjects = views
          .Union(ownersDb)
          .ToList();

      return ConvertMarkerListDB2DTO(allObjects);
    }


    public async Task<FiguresDTO> GetFigures(Dictionary<string, GeoObjectDTO> geo)
    {
      var result = new FiguresDTO();

      if (geo == null)
      {
        return result;
      }

      var ids = geo.Keys.ToList();

      var tree = await GetAsync(ids);

      var props = await GetPropsAsync(ids);

      foreach (var item in tree.Values)
      {
        if (geo.TryGetValue(item.id, out var geoPart))
        {
          FigureZoomedDTO retItem = null;

          var figure = new FigureGeoDTO();
          figure.radius = geoPart.radius;
          figure.geometry = geoPart.location;
          result.figs.Add(figure);
          retItem = figure;

          if (retItem != null)
          {
            retItem.id = item.id;
            retItem.name = item.name;
            retItem.parent_id = item.parent_id;
            retItem.zoom_level = geoPart.zoom_level?.ToString();

            if (props.TryGetValue(retItem.id, out var objProp))
            {
              if (retItem.extra_props != null)
              {
                retItem.extra_props.AddRange(objProp.extra_props);
              }
              else
              {
                retItem.extra_props = objProp.extra_props;
              }
            }
          }

        }
      }
      return result;
    }
  }
}
