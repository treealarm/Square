using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

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
    [Route("GetAlarmedStates")]
    public async Task<List<AlarmState>> GetAlarmedStates(List<string> ids)
    {
      var owners_and_views = await _mapService.GetOwnersAndViewsAsync(ids);

      var owners = owners_and_views.Where(i => string.IsNullOrEmpty(i.Value.owner_id)).ToDictionary();

      var data = await _stateService.GetAlarmStatesAsync(owners.Keys.ToList());

      var retval = new List<AlarmState>();

      foreach (var id in ids)
      {
        if (owners_and_views.TryGetValue(id, out var view))
        {
          if (data.TryGetValue(id, out var state))
          {
            retval.Add(state);
          }
          else if (!string.IsNullOrEmpty(view.owner_id))
          {
            if (data.TryGetValue(view.owner_id, out var state_owner))
            {
              retval.Add(new AlarmState()
              {
                id = id,
                alarm = state_owner.alarm               
              }
              );
            }
          }
        }
      }
      return retval;
    }

    [HttpPost()]
    [Route("GetStates")]
    public async Task<List<ObjectStateDTO>> GetStates(List<string> ids)
    {
      var owners_and_views = await _mapService.GetOwnersAndViewsAsync(ids);

      var owners = owners_and_views.Where(i => string.IsNullOrEmpty(i.Value.owner_id)).ToDictionary();

      var data = await _stateService.GetStatesAsync(owners.Keys.ToList());

      var retval = new List<ObjectStateDTO>();

      foreach (var id in ids)
      {
        if (owners_and_views.TryGetValue(id, out var view)) 
        { 
          if (data.TryGetValue(id, out var state))
          {
            retval.Add(state);
          }
          else if (!string.IsNullOrEmpty(view.owner_id))
          {
            if (data.TryGetValue(view.owner_id, out var state_owner))
            {
              retval.Add(new ObjectStateDTO()
                {
                  id = id,
                  states = state_owner.states,
                  timestamp = state_owner.timestamp,
                }
              );
            }
          }
        }
      }
      return retval;
    }

    [HttpPost]
    [Route("GetVisualStates")]

    public async Task<ActionResult<MarkersVisualStatesDTO>> GetVisualStates(List<string> objIds)
    {
      var objsToUpdate = await _mapService.GetAsync(objIds);
      var objStates = await GetStates(objIds);
      var alarmStates = await GetAlarmedStates(objIds);

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
      vStateDTO.alarmed_objects = alarmStates;

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
