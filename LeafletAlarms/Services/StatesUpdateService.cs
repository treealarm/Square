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
    private readonly IIdsQueue _stateIdsQueueService;

    public StatesUpdateService(
      IStateService stateService,
      IIdsQueue hierarhyStateService,
      IPubSubService pubsub
    )
    {
      _stateService = stateService;
      _pubsub = pubsub;
      _stateIdsQueueService = hierarhyStateService;
    }

    public async Task<bool> UpdateStates(List<ObjectStateDTO> newObjs)
    {
      await _stateService.UpdateStatesAsync(newObjs);
      _pubsub.PublishNoWait(Topics.OnStateChanged, JsonSerializer.Serialize(newObjs));
      _stateIdsQueueService.AddIds(newObjs.Select(st => st.id).ToList());

      return true;
    }
  }
}
