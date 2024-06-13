using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class StatesController : ControllerBase
  {
    private readonly IStateService _stateService;
    private readonly IMapService _mapService;
    private readonly IStatesUpdateService _statesUpdateService;
    public StatesController(
      IStateService stateService,
      IMapService mapService,
      IStatesUpdateService statesUpdateService
    )
    {
      _stateService = stateService;
      _mapService = mapService;
      _statesUpdateService = statesUpdateService;
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
    [Route("GetVisualStates")]

    public async Task<ActionResult<MarkersVisualStatesDTO>> GetVisualStates(List<string> objIds)
    {
      var objsToUpdate = await _mapService.GetAsync(objIds);
      var objStates = await _stateService.GetStatesAsync(objIds);
      var alarmStates = await _stateService.GetAlarmStatesAsync(objIds);

      var setStateDescriptions = new HashSet<string>();

      foreach (var objState in objStates)
      {
        BaseMarkerDTO objToUpdate = null;
        objsToUpdate.TryGetValue(objState.id, out objToUpdate);

        if (objToUpdate == null)
        {
          continue;
        }

        setStateDescriptions.UnionWith(objState.states);
      }

      MarkersVisualStatesDTO vStateDTO = new MarkersVisualStatesDTO();
      vStateDTO.states_descr = new List<ObjectStateDescriptionDTO>();
      vStateDTO.alarmed_objects = alarmStates.Values.ToList();

      vStateDTO.states_descr.AddRange(
          await _stateService.GetStateDescrAsync(setStateDescriptions.ToList()));

      vStateDTO.states = objStates;

      return CreatedAtAction(nameof(GetVisualStates), vStateDTO);
    }

    [HttpPost]
    [Route("UpdateStates")]
    public async Task<ActionResult<int>> UpdateStates(List<ObjectStateDTO> newObjs)
    {
      await _statesUpdateService.UpdateStates(newObjs);

      return CreatedAtAction(nameof(UpdateStates), StatusCodes.Status200OK);
    }

    [HttpPost]
    [Route("UpdateStateDescrs")]
    public async Task<ActionResult<int>> UpdateStateDescrs(List<ObjectStateDescriptionDTO> newObjs)
    {
      await _statesUpdateService.UpdateStateDescrs(newObjs);

      return CreatedAtAction(nameof(UpdateStateDescrs), StatusCodes.Status200OK);
    }
  }
}
