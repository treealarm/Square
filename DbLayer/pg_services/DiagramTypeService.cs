using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class DiagramTypeService: IDiagramTypeService
  {
    private readonly PgDbContext _dbContext;
    public DiagramTypeService(PgDbContext context)
    {
      _dbContext = context;
    }


    public async Task DeleteAsync(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return;

      // конвертируем строки в гуиды (пропускаем те, что невалидные)
      var guidIds = ids
          .Select(s => Domain.Utils.ConvertObjectIdToGuid(s))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (guidIds.Count == 0)
        return;

      var toDelete = await _dbContext.DiagramTypes
                                     .Where(d => guidIds.Contains(d.id))
                                     .ToListAsync();

      if (toDelete.Count > 0)
      {
        _dbContext.DiagramTypes.RemoveRange(toDelete);
        await _dbContext.SaveChangesAsync();
      }
    }


    public async Task<Dictionary<string, DiagramTypeDTO>> GetListByTypeNamesAsync(List<string> typeNames)
    {
      if (typeNames == null || typeNames.Count == 0)
        return new Dictionary<string, DiagramTypeDTO>();

      try
      {
        var obj = await _dbContext.DiagramTypes
                                  .Where(d => typeNames.Contains(d.name))
                                  .Include(d => d.regions)
                                  .ToListAsync();

        return ConvertListDB2DTO(obj);
      }
      catch (Exception)
      {
        // в оригинале у тебя try/catch был пустым → оставляем так
        return new Dictionary<string, DiagramTypeDTO>();
      }
    }


    public async Task<Dictionary<string, DiagramTypeDTO>> GetListByTypeIdsAsync(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return new Dictionary<string, DiagramTypeDTO>();

      try
      {
        // конвертим string → Guid
        var guidIds = ids
            .Select(s => Domain.Utils.ConvertObjectIdToGuid(s))
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

        if (guidIds.Count == 0)
          return new Dictionary<string, DiagramTypeDTO>();

        var obj = await _dbContext.DiagramTypes
                                  .Where(d => guidIds.Contains(d.id))
                                  .Include(d => d.regions)
                                  .ToListAsync();

        return ConvertListDB2DTO(obj);
      }
      catch (Exception)
      {
        return new Dictionary<string, DiagramTypeDTO>();
      }
    }


    public async Task<Dictionary<string, DiagramTypeDTO>> GetDiagramTypesByFilter(GetDiagramTypesByFilterDTO filterDto)
    {
      if (filterDto == null)
        return new Dictionary<string, DiagramTypeDTO>();

      try
      {
        IQueryable<DBDiagramType> query = _dbContext.DiagramTypes;

        // --- фильтр по start_id (пагинация) ---
        if (!string.IsNullOrEmpty(filterDto.start_id))
        {
          var startGuid = Domain.Utils.ConvertObjectIdToGuid(filterDto.start_id);
          if (startGuid.HasValue)
          {
            if (filterDto.forward)
              query = query.Where(x => x.id.CompareTo(startGuid.Value) > 0);
            else
              query = query.Where(x => x.id.CompareTo(startGuid.Value) < 0);
          }
        }

        // --- фильтр по имени (аналог regex) ---
        if (!string.IsNullOrEmpty(filterDto.filter))
        {
          // В Postgres лучше ILIKE (регистронезависимый поиск по подстроке)
          query = query.Where(x => EF.Functions.ILike(x.name, $"%{filterDto.filter}%"));
        }

        // --- сортировка и лимит ---
        if (filterDto.forward)
        {
          query = query.OrderBy(x => x.id);
        }
        else
        {
          query = query.OrderByDescending(x => x.id);
        }

        query = query.Take(filterDto.count);

        var obj = await query.ToListAsync();

        // если шли назад — перевернём список в «правильный» порядок
        if (!filterDto.forward)
        {
          obj.Reverse();
        }

        return ConvertListDB2DTO(obj);
      }
      catch (Exception)
      {
        return new Dictionary<string, DiagramTypeDTO>();
      }
    }

    public async Task UpdateListAsync(List<DiagramTypeDTO> newObjs)
    {
      if (newObjs == null || newObjs.Count == 0)
        return;

      // Конвертим DTO → DB и собираем пары для удобства
      var dtoIdPairs = newObjs
          .Select(dto =>
          {
            Guid id = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Domain.Utils.NewGuid();
            return new { Id = id, Dto = dto };
          })
          .ToList();

      var ids = dtoIdPairs.Select(x => x.Id).ToList();

      // Загружаем существующие объекты из БД вместе с регионами
      var existing = await _dbContext.DiagramTypes
                                     .Include(d => d.regions)
                                     .Where(d => ids.Contains(d.id))
                                     .ToListAsync();

      var existingDict = existing.ToDictionary(x => x.id, x => x);

      var toAdd = new List<DBDiagramType>();

      foreach (var pair in dtoIdPairs)
      {
        if (existingDict.TryGetValue(pair.Id, out var dbObj))
        {
          // --- обновление родительского объекта ---
          dbObj.name = pair.Dto.name;
          dbObj.src = pair.Dto.src;

          var incomingRegions = pair.Dto.regions ?? new List<DiagramTypeRegionDTO>();

          // --- удаляем регионы, которых нет в incoming ---
          var incomingKeys = incomingRegions
              .Select(r => r.region_key)
              .ToHashSet();

          var toRemove = dbObj.regions
              .Where(r => !incomingKeys.Contains(r.region_key))
              .ToList();

          foreach (var r in toRemove)
          {
            dbObj.regions.Remove(r);
            _dbContext.DiagramTypeRegions.Remove(r);
          }

          // --- обновляем существующие регионы и добавляем новые ---
          foreach (var r in incomingRegions)
          {
            var existingRegion = dbObj.regions.FirstOrDefault(x => x.region_key == r.region_key);

            if (existingRegion != null)
            {
              // обновляем существующий регион
              existingRegion.geometry = new DBDiagramCoord
              {
                top = r.geometry.top,
                left = r.geometry.left,
                width = r.geometry.width,
                height = r.geometry.height
              };
              existingRegion.styles = r.styles;
            }
            else
            {
              // добавляем новый регион
              var newRegion = new DBDiagramTypeRegion
              {
                diagram_type_id = dbObj.id,
                region_key = r.region_key,
                geometry = new DBDiagramCoord
                {
                  top = r.geometry.top,
                  left = r.geometry.left,
                  width = r.geometry.width,
                  height = r.geometry.height
                },
                styles = r.styles
              };

              _dbContext.DiagramTypeRegions.Add(newRegion);
              dbObj.regions.Add(newRegion);
            }
          }
        }
        else
        {
          // --- вставка нового родителя вместе с регионами ---
          var newDbObj = new DBDiagramType
          {
            id = pair.Id,
            name = pair.Dto.name,
            src = pair.Dto.src,
            regions = pair.Dto.regions?.Select(r => new DBDiagramTypeRegion
            {
              diagram_type_id = pair.Id,
              region_key = r.region_key,
              geometry = new DBDiagramCoord
              {
                top = r.geometry.top,
                left = r.geometry.left,
                width = r.geometry.width,
                height = r.geometry.height
              },
              styles = r.styles
            }).ToList()
          };

          toAdd.Add(newDbObj);
        }
      }

      if (toAdd.Count > 0)
        await _dbContext.DiagramTypes.AddRangeAsync(toAdd);

      await _dbContext.SaveChangesAsync();

      // выставляем id обратно в DTO
      foreach (var pair in dtoIdPairs)
      {
        pair.Dto.id = pair.Id.ToString();
      }
    }



    DBDiagramType ConvertDTO2DB(DiagramTypeDTO dto)
    {
      if (dto == null)
      {
        return null;
      }

      var dbo = new DBDiagramType()
      {
        id = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Domain.Utils.NewGuid(),
        regions = new List<DBDiagramTypeRegion>(),
        src = dto.src,
        name = dto.name
      };

      if (dto.regions != null)
      {
        foreach (var item in dto.regions)
        {
          dbo.regions.Add(new DBDiagramTypeRegion()
          {
            geometry = item.geometry.CopyAll<DiagramCoordDTO, DBDiagramCoord>(),
            region_key = item.region_key,
            styles = new Dictionary<string, string>(item.styles)
          });
        }
      }      

      return dbo;
    }

    Dictionary<string, DiagramTypeDTO> ConvertListDB2DTO(List<DBDiagramType> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new Dictionary<string, DiagramTypeDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new DiagramTypeDTO()
        {
          id = Domain.Utils.ConvertGuidToObjectId(dbObj.id),
          regions = new List<DiagramTypeRegionDTO>(),
          src = dbObj.src,
          name = dbObj.name,
        };

        if (dbObj.regions != null)
        {
          foreach (var item in dbObj.regions)
          {
            dto.regions.Add(new DiagramTypeRegionDTO()
            {
              region_key = item.region_key,
              geometry = item.geometry.CopyAll<DBDiagramCoord, DiagramCoordDTO>(),
              styles = new Dictionary<string, string>(item.styles)
            });
          }
        }

        newObjs.Add(dto.id, dto);
      }
      return newObjs;
    }
  }
}
