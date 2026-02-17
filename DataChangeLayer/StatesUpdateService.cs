
using Domain;
using System.Linq;

namespace DataChangeLayer
{
  internal class StatesUpdateService: IStatesUpdateService
  {
    private readonly IStateService _stateService;
    private readonly IMapService _mapService;
    private static HashSet<string>? _alarmDescr;
    private static readonly SemaphoreSlim _initLock = new(1, 1);
    IPubService _pub;

    public StatesUpdateService(
      IMapService mapService,
      IStateService stateService,
      IPubService pub
    )
    {
      _stateService = stateService;
      _mapService = mapService;
      _pub = pub;
    }

    private async Task<HashSet<string>?> GetAllAlarmStates()
    {
      if (_alarmDescr != null)
        return _alarmDescr;

      await _initLock.WaitAsync();
      try
      {
        if (_alarmDescr == null)
        {
          var descrs = await _stateService.GetAlarmStatesDescr(null) ?? null;

          if (descrs != null)
          {
            _alarmDescr = descrs
              .Where(s => !string.IsNullOrEmpty(s.Value.state))
              .Select(s => s.Value.state!)
              .ToHashSet();
          }          
        }
      }
      finally
      {
        _initLock.Release();
      }

      return _alarmDescr;
    }

    public async Task<bool> UpdateStates(List<ObjectStateDTO> objStates)
    {
      if (!objStates.Any())
      {
        return true;
      }
      //First select only owners
      List<string> ids = objStates.Select(i => i.id ?? "").ToList();
      var objsToUpdate = await _mapService.GetOwnersAsync(ids);
      objStates.RemoveAll(i => !objsToUpdate.ContainsKey(i.id ?? string.Empty));
      
      await _stateService.UpdateStatesAsync(objStates);

      //Now let notify about alarm changes
      var allAlarm = await GetAllAlarmStates();

      if (allAlarm != null)
      {
        var newAlarmedObjs = objStates
          .Where(o => o.states != null && o.states.Any(s => allAlarm.Contains(s)))
          .Select(o=>o.id)
          .ToHashSet();

        var processingObjs = objStates.Select(st => st.id).ToList();
        var currentStates = await _stateService.GetAlarmedLeafStatesAsync(processingObjs!);

        var currentAlarmedObjs = currentStates
          .Where(s => s.alarm == true)
          .Select(o => o.id)
          .ToHashSet();

        var clearedAlarms = currentAlarmedObjs
            .Where(id => !newAlarmedObjs.Contains(id))
            .ToHashSet();

        // Составляем список AlarmState
        var alarmStates = new List<AlarmBaseState>();

        // Новые тревожные → alarm = true
        alarmStates.AddRange(newAlarmedObjs.Select(id => new AlarmBaseState { id = id, alarm = true }));

        // Снятые тревоги → alarm = false
        alarmStates.AddRange(clearedAlarms.Select(id => new AlarmBaseState { id = id, alarm = false }));

        if (alarmStates.Count > 0)
        {
          await _pub.Publish(Topics.AlarmStatesChanged, alarmStates);
        }        
      }
 
      return true;
    }

    public async Task<long> UpdateStateDescrs(List<ObjectStateDescriptionDTO> newObjs)
    {
      return await _stateService.UpdateStateDescrsAsync(newObjs);
    }

    public async Task UpdateAlarmStatesAsync(List<AlarmState> alarms)
    {
      await _stateService.UpdateAlarmStatesAsync(alarms);
    }

    public async Task DropStateAlarms()
    {
      await _stateService.DropStateAlarms();
    }
  }
}
