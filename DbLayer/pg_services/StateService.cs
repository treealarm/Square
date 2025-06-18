using Microsoft.EntityFrameworkCore;
using DbLayer.Models;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class StateService : IStateService
  {
    private readonly PgDbContext _dbContext;

    public StateService(PgDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task Init()
    {
      if (!await _dbContext.ObjectStateDescriptions.AnyAsync())
      {
        var defaults = new List<DBObjectStateDescription>
        {
          new() { state = "ALARM", state_descr = "Alarm", state_color = "#FF0000", alarm = true },
          new() { state = "INFO",  state_descr = "Info",  state_color = "#00FF00", alarm = false },
          new() { state = "NORM",  state_descr = "Normal", state_color = "#0000FF", alarm = false }
        };

        _dbContext.ObjectStateDescriptions.AddRange(defaults);
        await _dbContext.SaveChangesAsync();
      }
    }

    public async Task<long> UpdateStatesAsync(List<ObjectStateDTO> newObjs)
    {
      if (newObjs.Count == 0)
        return 0;
 
      var idsToUpdate = newObjs
          .Select(x => Utils.ConvertObjectIdToGuid(x.id))
          .ToList();

      var existing = await _dbContext.ObjectStates
          .Include(x => x.states)
          .Where(x => idsToUpdate.Contains(x.id))
          .ToListAsync();

      var existingDict = existing.ToDictionary(x => x.id);

      var toUpdate = new List<DBObjectState>();
      var toAdd = new List<DBObjectState>();

      foreach (var dto in newObjs)
      {
        var objId = Utils.ConvertObjectIdToGuid(dto.id) ?? Guid.Empty;

        var states = dto.states.Select(s => new DBObjectStateValue
        {
          state = s,
          object_id = objId
        }).ToList();

        if (existingDict.TryGetValue(objId, out var existingEntity))
        {
          existingEntity.states.Clear();
          existingEntity.timestamp = DateTime.UtcNow;

          // Удаляем старые states, заменяем новыми
          //_dbContext.ObjectStateValues.RemoveRange(existingEntity.states);
          existingEntity.states.AddRange(states);

          toUpdate.Add(existingEntity);
        }
        else
        {
          var newEntity = new DBObjectState
          {
            id = objId,
            timestamp = DateTime.UtcNow,
            states = states
          };

          toAdd.Add(newEntity);
        }
      }

      _dbContext.ObjectStates.UpdateRange(toUpdate);
      await _dbContext.ObjectStates.AddRangeAsync(toAdd);
      return await _dbContext.SaveChangesAsync();
    }


    public async Task<long> UpdateStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs)
    {
      var states = newObjs.Select(x => x.state).ToList();
      var existing = await _dbContext.ObjectStateDescriptions
        .Where(x => states.Contains(x.state))
        .ToListAsync();

      _dbContext.ObjectStateDescriptions.RemoveRange(existing);

      var newEntities = newObjs.Select(x => new DBObjectStateDescription
      {
        id = Utils.ConvertObjectIdToGuid(x.id) ?? Guid.Empty,
        state = x.state,
        state_descr = x.state_descr,
        state_color = x.state_color,
        alarm = x.alarm ?? false
      });

      await _dbContext.ObjectStateDescriptions.AddRangeAsync(newEntities);
      return await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<string, ObjectStateDTO>> GetStatesAsync(List<string> ids)
    {
      var parsedIds = ids.Select(Utils.ConvertObjectIdToGuid).ToList();

      var states = await _dbContext.ObjectStates
        .Include(x => x.states)
        .Where(x => parsedIds.Contains(x.id))
        .ToListAsync();

      return states.ToDictionary(
        x => Utils.ConvertGuidToObjectId(x.id),
        x => new ObjectStateDTO
        {
          id = Utils.ConvertGuidToObjectId(x.id),
          states = x.states.Select(s => s.state).ToList(),
          timestamp = x.timestamp
        });
    }

    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(List<string> states)
    {
      var result = await _dbContext.ObjectStateDescriptions
        .Where(x => states.Contains(x.state))
        .ToListAsync();

      return result.Select(x => new ObjectStateDescriptionDTO
      {
        id = Utils.ConvertGuidToObjectId(x.id),
        state = x.state,
        state_descr = x.state_descr,
        state_color = x.state_color,
        alarm = x.alarm
      }).ToList();
    }

    public async Task UpdateAlarmStatesAsync(List<AlarmState> alarms)
    {
      foreach (var alarm in alarms)
      {
        var id = Utils.ConvertObjectIdToGuid(alarm.id) ?? Guid.Empty;
        var existing = await _dbContext.AlarmStates.FindAsync(id);

        if (existing != null)
        {
          existing.alarm = alarm.alarm ?? false;
        }
        else
        {
          _dbContext.AlarmStates.Add(new DBAlarmState
          {
            id = id,
            alarm = alarm.alarm ?? false
          });
        }
      }

      await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<string, AlarmState>> GetAlarmStatesAsync(List<string> ids)
    {
      var parsedIds = ids.Select(Utils.ConvertObjectIdToGuid).ToList();

      var result = await _dbContext.AlarmStates
        .Where(x => parsedIds.Contains(x.id))
        .ToListAsync();

      return result.ToDictionary(
        x => Utils.ConvertGuidToObjectId(x.id),
        x => new AlarmState
        {
          id = Utils.ConvertGuidToObjectId(x.id),
          alarm = x.alarm
        });
    }

    public async Task DropStateAlarms()
    {
      var all = await _dbContext.AlarmStates.ToListAsync();
      _dbContext.AlarmStates.RemoveRange(all);
      await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<string, ObjectStateDescriptionDTO>> GetAlarmStatesDescr(List<string> states)
    {
      var query = _dbContext.ObjectStateDescriptions.Where(x => x.alarm);

      if (states != null && states.Count > 0)
      {
        query = query.Where(x => states.Contains(x.state));
      }

      var list = await query.ToListAsync();

      return list.ToDictionary(
        x => x.state,
        x => new ObjectStateDescriptionDTO
        {
          id = Utils.ConvertGuidToObjectId(x.id),
          state = x.state,
          state_descr = x.state_descr,
          state_color = x.state_color,
          alarm = x.alarm
        });
    }

    public async Task<Dictionary<string, ObjectStateDTO>> GetAlarmedStates(List<string> statesFilter)
    {
      var alarmedDescr = await GetAlarmStatesDescr(statesFilter);
      var alarmedStates = alarmedDescr.Keys.ToHashSet();

      var result = await _dbContext.ObjectStates
        .Include(x => x.states)
        .Where(x => x.states.Any(s => alarmedStates.Contains(s.state)))
        .ToListAsync();

      return result.ToDictionary(
        x => Utils.ConvertGuidToObjectId(x.id),
        x => new ObjectStateDTO
        {
          id = Utils.ConvertGuidToObjectId(x.id),
          states = x.states.Select(s => s.state).ToList()
        });
    }
  }
}
