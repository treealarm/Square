using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class StatesController : ControllerBase
  {
    private readonly IStateService _stateService;
    private readonly IStateConsumer _stateConsumerService;
    private readonly IMapService _mapService;
    private readonly IHierarhyStateService _hierarhyStateService;
    public StatesController(
      IStateService stateService,
      IStateConsumer stateConsumerService,
      IMapService mapService,
      IHierarhyStateService hierarhyStateService
    )
    {
      _stateService = stateService;
      _stateConsumerService = stateConsumerService;
      _mapService = mapService;
      _hierarhyStateService = hierarhyStateService;
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
      await _hierarhyStateService.OnStatesChanged(newObjs);

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
      Dictionary<string, List<string>> mapExTypeToStates = new Dictionary<string, List<string>>();

      foreach (var objState in objStates)
      {
        var objToUpdate = objsToUpdate.Where(o => o.id == objState.id).FirstOrDefault();

        if (objToUpdate == null)
        {
          continue;
        }

        List<string> listOfStates;

        if (objToUpdate.external_type == null)
        {
          objToUpdate.external_type = String.Empty;
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
