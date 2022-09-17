using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
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
    public StatesController(IStateService stateService)
    {
      _stateService = stateService;
    }

    [HttpPost()]
    [Route("GetStateDescr")]
    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescr(
      List<string> states
    )
    {
      var data = await _stateService.GetStateDescrAsync(states);
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
    [Route("InsertStates")]
    public async Task<ActionResult<int>> InsertStates(List<ObjectStateDTO> newObjs)
    {
      await _stateService.InsertStatesAsync(newObjs);
      return CreatedAtAction(nameof(InsertStates), StatusCodes.Status200OK);
    }

    [HttpPost]
    [Route("InsertStateDescrs")]
    public async Task<ActionResult<int>> InsertStateDescrs(List<ObjectStateDescriptionDTO> newObjs)
    {
      await _stateService.InsertStateDescrsAsync(newObjs);

      return CreatedAtAction(nameof(InsertStateDescrs), StatusCodes.Status200OK);
    }
  }
}
