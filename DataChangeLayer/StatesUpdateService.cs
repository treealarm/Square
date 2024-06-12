
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Domain.States;

namespace DataChangeLayer
{
  public class StatesUpdateService: IStatesUpdateService
  {
    private readonly IStateService _stateService;
    IPubService _pub;

    public StatesUpdateService(
      IStateService stateService,
      IPubService pub
    )
    {
      _stateService = stateService;
      _pub = pub;
    }

    public async Task<bool> UpdateStates(List<ObjectStateDTO> newObjs)
    {
      await _stateService.UpdateStatesAsync(newObjs);
      await _pub.Publish(Topics.OnStateChanged, newObjs);
      await _pub.Publish(Topics.CheckStatesByIds, newObjs.Select(st => st.id).ToList());

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
