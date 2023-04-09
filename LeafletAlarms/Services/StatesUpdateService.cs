using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Services
{
  public class StatesUpdateService
  {
    private readonly IStateService _stateService;
    private readonly IStateConsumer _stateConsumerService;
    private readonly IMapService _mapService;
    private readonly IIdsQueue _stateIdsQueueService;

    public StatesUpdateService(
      IStateService stateService,
      IStateConsumer stateConsumerService,
      IMapService mapService,
      IIdsQueue hierarhyStateService
    )
    {
      _stateService = stateService;
      _stateConsumerService = stateConsumerService;
      _mapService = mapService;
      _stateIdsQueueService = hierarhyStateService;
    }

    public async Task<bool> UpdateStates(List<ObjectStateDTO> newObjs)
    {
      await _stateConsumerService.OnStateChanged(newObjs);
      await _stateService.UpdateStatesAsync(newObjs);
      _stateIdsQueueService.AddIds(newObjs.Select(st => st.id).ToList());

      return true;
    }
  }
}
