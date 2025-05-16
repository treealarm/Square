using DbLayer.Models.Actions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    private DBActionExeResult ConvertDTO2DBResult(ActionExeResultDTO dto)
    {
      var retVal = new DBActionExeResult();
      retVal.progress = dto.progress??100;

      JsonElement curValJson = JsonSerializer.SerializeToElement(dto.result ?? new { });
      retVal.result = curValJson;
      retVal.id = Guid.Parse(dto.action_execution_id);
      return retVal;
    }
    private DBActionExe ConvertDTO2DB(ActionExeDTO dto)
    {
      if (string.IsNullOrEmpty(dto.action_execution_id))
      {
        dto.action_execution_id = Guid.NewGuid().ToString();
      }
      var ret = new DBActionExe
      {
        id = Guid.Parse(dto.action_execution_id),
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
        .Where(a => !string.IsNullOrEmpty(a.action_execution_id) && Guid.TryParse(a.action_execution_id, out _))
        .Select(a => Guid.Parse(a.action_execution_id))
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

    public async Task<List<ActionExeInfoDTO>> GetActionsByObjectId(string objectId)
    {
      var ret = new List<ActionExeInfoDTO>();
      var objectGuid = Utils.ConvertObjectIdToGuid(objectId);

      var existing = await _dbContext.Actions
        .Where(a => a.object_id == objectGuid)
        .ToListAsync();

      if (existing == null || existing.Count == 0)
      {
        return ret;
      }
      var actionIds = existing.Select(a=>a.id).ToList();

      var results = await _dbContext.ActionResults
        .Where(r => actionIds.Contains(r.id))
        .ToListAsync();

      foreach (var result in results)
      {
        var action  = existing.Where(a=>a.id ==  result.id).FirstOrDefault();
        if (action == null)
        {
          continue;
        }

        var info = new ActionExeInfoDTO();
        info.name = action.name;
        info.object_id = Utils.ConvertGuidToObjectId(action.object_id);
        ret.Add(info);

        info.result = new ActionExeResultDTO()
        {
          progress = result.progress,
          result = result.result.ToString(),
          action_execution_id = result.id.ToString()
        };
      }
      return ret;
    }

    public async Task UpdateResultsAsync(List<ActionExeResultDTO> results)
    {
      if (results.Count == 0)
        return;

      // 1. Собираем ID из входящего списка
      var idsToUpdate = results
        .Where(a => !string.IsNullOrEmpty(a.action_execution_id) && Guid.TryParse(a.action_execution_id, out _))
        .Select(a => Guid.Parse(a.action_execution_id))
        .ToList();

      // 2. Загружаем уже существующие записи из БД
      var existing = await _dbContext.ActionResults
        .Where(a => idsToUpdate.Contains(a.id))
        .ToListAsync();

      var existingDict = existing.ToDictionary(a => a.id);

      // 3. Разделяем на добавление и обновление
      var toUpdate = new List<DBActionExeResult>();
      var toAdd = new List<DBActionExeResult>();

      foreach (var dto in results)
      {
        var entity = ConvertDTO2DBResult(dto);

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

      _dbContext.ActionResults.UpdateRange(toUpdate);
      await _dbContext.ActionResults.AddRangeAsync(toAdd);
      await _dbContext.SaveChangesAsync();
    }
  }
}
