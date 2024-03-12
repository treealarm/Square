using Domain.ServiceInterfaces;
using Domain.States;
using PubSubLib;
using System.Text.Json;

namespace LeafletAlarms.Services
{
  public class StatesUpdateService
  {
    private readonly IStateService _stateService;
    IPubSubService _pubsub;

    public StatesUpdateService(
      IStateService stateService,
      IPubSubService pubsub
    )
    {
      _stateService = stateService;
      _pubsub = pubsub;
    }

    public async Task<bool> UpdateStates(List<ObjectStateDTO> newObjs)
    {
      await _stateService.UpdateStatesAsync(newObjs);
      await _pubsub.Publish(Topics.OnStateChanged, JsonSerializer.Serialize(newObjs));
      await _pubsub.Publish(Topics.CheckStatesByIds, JsonSerializer.Serialize(newObjs.Select(st => st.id).ToList()));

      return true;
    }
  }
}
