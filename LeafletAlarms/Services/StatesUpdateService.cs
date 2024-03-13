using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Domain.States;
using PubSubLib;
using System.Text.Json;

namespace LeafletAlarms.Services
{
  public class StatesUpdateService
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
  }
}
