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
    private readonly StatesUpdateService _statesUpdateService;
    public StatesController(
      IStateService stateService,
      IMapService mapService,
      StatesUpdateService statesUpdateService
    )
    {
      _stateService = stateService;
      _mapService = mapService;
      _statesUpdateService = statesUpdateService;
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
      await _statesUpdateService.UpdateStates(newObjs);

      return CreatedAtAction(nameof(UpdateStates), StatusCodes.Status200OK);
    }

    [HttpPost]
    [Route("UpdateStateDescrs")]
    public async Task<ActionResult<int>> UpdateStateDescrs(List<ObjectStateDescriptionDTO> newObjs)
    {
      await _stateService.UpdateStateDescrsAsync(newObjs);

      return CreatedAtAction(nameof(UpdateStateDescrs), StatusCodes.Status200OK);
    }

    [HttpPost]
    [Route("GetVisualStates")]

    public async Task<ActionResult<MarkersVisualStatesDTO>> GetVisualStates(List<string> objIds)
    {
      var objsToUpdate = await _mapService.GetAsync(objIds);
      var objStates = await _stateService.GetStatesAsync(objIds);
      var alarmStates = await _stateService.GetAlarmStatesAsync(objIds);

      Dictionary<string, List<string>> mapExTypeToStates = new Dictionary<string, List<string>>();

      foreach (var objState in objStates)
      {
        BaseMarkerDTO objToUpdate = null;
        objsToUpdate.TryGetValue(objState.id, out objToUpdate);

        if (objToUpdate == null)
        {
          continue;
        }

        List<string> listOfStates;

        if (objToUpdate.external_type == null)
        {
          objToUpdate.external_type = string.Empty;
        }

        if (!mapExTypeToStates.TryGetValue(objToUpdate.external_type, out listOfStates))
        {
          listOfStates = new List<string>();
          mapExTypeToStates.Add(objToUpdate.external_type, listOfStates);
        }

        listOfStates.AddRange(objState.states);
      }

      MarkersVisualStatesDTO vStateDTO = new MarkersVisualStatesDTO();
      vStateDTO.states_descr = new List<ObjectStateDescriptionDTO>();
      vStateDTO.alarmed_objects = alarmStates.Values.ToList();

      foreach (var pair in mapExTypeToStates)
      {
        vStateDTO.states_descr.AddRange(
          await _stateService.GetStateDescrAsync(pair.Key, pair.Value));
      }

      vStateDTO.states = objStates;

      return CreatedAtAction(nameof(GetVisualStates), vStateDTO);
    }

    
  }
}
