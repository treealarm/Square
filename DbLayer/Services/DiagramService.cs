using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class DiagramService : IDiagramService, IDiagramServiceInternal
  {
    private readonly PgDbContext _dbContext;
    public DiagramService(PgDbContext context)
    {
      _dbContext = context;
    }

    async Task IDiagramServiceInternal.RemoveAsync(List<string> ids)
    {
      var guidIds = ids
          .Select(s => Domain.Utils.ConvertObjectIdToGuid(s))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (guidIds.Count == 0)
        return;

      var diagrams = await _dbContext.Diagrams
          .Where(x => guidIds.Contains(x.id))
          .ToListAsync();

      if (diagrams.Count > 0)
      {
        _dbContext.Diagrams.RemoveRange(diagrams);
        await _dbContext.SaveChangesAsync();
      }
    }



    async Task<Dictionary<string, DiagramDTO>> IDiagramService.GetListByIdsAsync(
        List<string> ids
    )
    {
      List<DBDiagram> obj = new();

      try
      {
        var guidIds = ids
            .Select(s => Domain.Utils.ConvertObjectIdToGuid(s))
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

        if (guidIds.Count > 0)
        {
          obj = await _dbContext.Diagrams
              .Where(s => guidIds.Contains(s.id))
              .ToListAsync();
        }
      }
      catch (Exception)
      {
        // Можно логировать, чтобы не потерять причину
      }

      return ConvertListDB2DTO(obj);
    }


    async Task IDiagramServiceInternal.UpdateListAsync(List<DiagramDTO> newObjs)
    {
      if (newObjs.Count == 0)
        return;

      // Словарь для сопоставления DTO → DB
      var dbUpdated = new Dictionary<DiagramDTO, DBDiagram>();

      // Конвертим DTO → DB, собираем все Guid
      var dbObjects = newObjs
          .Select(item =>
          {
            var dbObj = ConvertDTO2DB(item);
            dbUpdated[item] = dbObj;
            return dbObj;
          })
          .ToList();

      var ids = dbObjects
          .Select(o => o.id)
          .Where(id => id != Guid.Empty)
          .ToList();

      // Загружаем существующие объекты из БД
      var existing = await _dbContext.Diagrams
          .Where(d => ids.Contains(d.id))
          .ToListAsync();

      var existingDict = existing.ToDictionary(e => e.id);

      foreach (var obj in dbObjects)
      {
        if (obj.id == Guid.Empty || !existingDict.ContainsKey(obj.id))
        {
          // Новая запись → генерируем Guid, если пустой
          if (obj.id == Guid.Empty)
            obj.id = Guid.NewGuid();

          _dbContext.Diagrams.Add(obj);
        }
        else
        {
          // Существующая запись → обновляем загруженный объект
          var existingObj = existingDict[obj.id];
          existingObj.dgr_type = obj.dgr_type;
          existingObj.geometry = obj.geometry;
          existingObj.region_id = obj.region_id;
          existingObj.background_img = obj.background_img;
        }
      }

      // Сохраняем все изменения
      await _dbContext.SaveChangesAsync();

      // Проставляем id обратно в DTO
      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id.ToString();
      }
    }



    DBDiagram ConvertDTO2DB(DiagramDTO dto)
    {
      if (dto == null)
      {
        return null;
      }

      var dbo = new DBDiagram()
      {
        id = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Domain.Utils.NewGuid(),
        dgr_type = dto.dgr_type,
        region_id = dto.region_id,
        background_img = dto.background_img,
        geometry = new DBDiagramCoord(),
      };

      dbo.geometry = new DBDiagramCoord()
      {
        height = dto.geometry.height,
        width = dto.geometry.width,
        left = dto.geometry.left,
        top = dto.geometry.top
      };
      return dbo;
    }

    Dictionary<string, DiagramDTO> ConvertListDB2DTO(List<DBDiagram> dbos)
    {
      if (dbos == null)
      {
        return null;
      }

      var newObjs = new Dictionary<string, DiagramDTO>();

      foreach (var dbObj in dbos)
      {
        var dto = new DiagramDTO()
        {
          id = Domain.Utils.ConvertGuidToObjectId(dbObj.id),
          dgr_type = dbObj.dgr_type,
          geometry = new DiagramCoordDTO(),
          background_img = dbObj.background_img,
          region_id = dbObj.region_id
        };

        dto.geometry = new DiagramCoordDTO
        {
          top = dbObj.geometry.top,
          left = dbObj.geometry.left,
          width = dbObj.geometry.width,
          height = dbObj.geometry.height
        };

        newObjs.Add(dto.id, dto);
      }
      return newObjs;
    }
  }
}
