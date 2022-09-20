using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class StatesController : ControllerBase
  {
    private readonly IStateService _stateService;
    private readonly IStateConsumer _stateConsumerService;
    public StatesController(
      IStateService stateService,
      IStateConsumer stateConsumerService
    )
    {
      _stateService = stateService;
      _stateConsumerService = stateConsumerService;
    }

    [HttpPost()]
    [Route("GetStateDescr")]
    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescr(
      string external_type,
      List<string> states
    )
    {
      var data = await _stateService.GetStateDescrAsync(external_type, states);
      return data;
    }

    [HttpPost()]
    [Route("GetStates")]
    public async Task<List<ObjectStateDTO>> GetStates(List<string> ids)
    {
      var data = await _stateService.GetStatesAsync(ids);
      return data;
    }

    [HttpPost]
    [Route("UpdateStates")]
    public async Task<ActionResult<int>> UpdateStates(List<ObjectStateDTO> newObjs)
    {
      await _stateConsumerService.OnStateChanged(newObjs);
      await _stateService.UpdateStatesAsync(newObjs);
      return CreatedAtAction(nameof(UpdateStates), StatusCodes.Status200OK);
    }

    [HttpPost]
    [Route("UpdateStateDescrs")]
    public async Task<ActionResult<int>> UpdateStateDescrs(List<ObjectStateDescriptionDTO> newObjs)
    {
      await _stateService.UpdateStateDescrsAsync(newObjs);

      return CreatedAtAction(nameof(UpdateStateDescrs), StatusCodes.Status200OK);
    }
  }
}
