using DbLayer.Models;
using DbLayer.Models.Actions;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class ActionsService: IActionsService, IActionsServiceInternal
  {
    private readonly PgDbContext _dbContext;

    public ActionsService(PgDbContext context)
    {
      _dbContext = context;
    }

    private DBActionExe ConvertDTO2DB(ActionExeDTO dto)
    {
      if (string.IsNullOrEmpty(dto.uid))
      {
        dto.uid = Guid.NewGuid().ToString();
      }
      var ret = new DBActionExe
      {
        id = Guid.Parse(dto.uid),
        object_id = Utils.ConvertObjectIdToGuid(dto.object_id!),
        name = dto.name,
        timestamp = DateTime.UtcNow
      };

      if (dto.parameters != null) 
      {
        ret.parameters = new List<DBActionParameter>();
        foreach (var param in dto.parameters)
        {
          JsonElement curValJson = JsonSerializer.SerializeToElement(param.cur_val ?? new { });

          ret.parameters.Add(new DBActionParameter()
          {
            action_execution_id = ret.id,
            name = param.name,
            type = param.type,
            cur_val = curValJson
          });
        }
      }
      
      return ret;
    }

    public async Task UpdateListAsync(List<ActionExeDTO> actions)
    {
      if (actions.Count == 0)
        return;

      // 1. Собираем ID из входящего списка
      var idsToUpdate = actions
        .Where(a => !string.IsNullOrEmpty(a.uid) && Guid.TryParse(a.uid, out _))
        .Select(a => Guid.Parse(a.uid))
        .ToList();

      // 2. Загружаем уже существующие записи из БД
      var existing = await _dbContext.Actions
        .Where(a => idsToUpdate.Contains(a.id))
        .ToListAsync();

      var existingDict = existing.ToDictionary(a => a.id);

      // 3. Разделяем на добавление и обновление
      var toUpdate = new List<DBActionExe>();
      var toAdd = new List<DBActionExe>();

      foreach (var dto in actions)
      {
        var entity = ConvertDTO2DB(dto);

        if (existingDict.TryGetValue(entity.id, out var existingEntity))
        {
          _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
          toUpdate.Add(existingEntity);
        }
        else
        {
          toAdd.Add(entity);
        }
      }

      _dbContext.Actions.UpdateRange(toUpdate);
      await _dbContext.Actions.AddRangeAsync(toAdd);
      await _dbContext.SaveChangesAsync();
    }
  }
}
