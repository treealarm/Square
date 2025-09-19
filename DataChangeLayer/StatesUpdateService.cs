
using Domain;

namespace DataChangeLayer
{
  internal class StatesUpdateService: IStatesUpdateService
  {
    private readonly IStateService _stateService;
    private readonly IMapService _mapService;
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

    public async Task<bool> UpdateStates(List<ObjectStateDTO> objStates)
    {
      if (!objStates.Any())
      {
        return true;
      }
      //First select only owners
      List<string> ids = objStates.Select(i => i.id ?? "").ToList();
      var objsToUpdate = await _mapService.GetOwnersAsync(ids);
      objStates.RemoveAll(i => !ids.Contains(i.id ?? string.Empty));

      await _stateService.UpdateStatesAsync(objStates);
      await _pub.Publish(Topics.OnStateChanged, objStates);
      await _pub.Publish(Topics.CheckStatesByIds, objStates.Select(st => st.id).ToList());

      return true;
    }

    public async Task<long> UpdateStateDescrs(List<ObjectStateDescriptionDTO> newObjs)
    {
      return await _stateService.UpdateStateDescrsAsync(newObjs);
    }

    public async Task UpdateAlarmStatesAsync(List<AlarmState> alarms)
    {
      await _stateService.UpdateAlarmStatesAsync(alarms);
      await _pub.Publish(Topics.OnBlinkStateChanged, alarms);
    }

    public async Task DropStateAlarms()
    {
      await _stateService.DropStateAlarms();
    }
  }
}
