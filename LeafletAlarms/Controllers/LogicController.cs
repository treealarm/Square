using Domain;
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Itinero;
using LeafletAlarms.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PubSubLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConstants.admin)]
  public class LogicController : ControllerBase
  {
    private readonly ILogicService _logicService;
    private IPubService _pub;
    public LogicController(
      ILogicService logicService,
      IPubService pubsub
    )
    {
      _logicService = logicService;
      _pub = pubsub;
    }

    [HttpPost]
    [Route("UpdateLogic")]
    public async Task<ActionResult<List<StaticLogicDTO>>> Update(List<StaticLogicDTO> newObjs)
    {
      foreach (var newObj in newObjs)
      {
        await _logicService.UpdateAsync(newObj);
      }

      var listIds = newObjs.Select(o => o.id).ToList();

      await _pub.Publish(Topics.UpdateLogicProc, listIds);

      return CreatedAtAction(nameof(Update), newObjs);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      try
      {
        await _logicService.DeleteAsync(id);        
      }
      catch(Exception ex)
      {
        return StatusCode(
          StatusCodes.Status500InternalServerError,
          ex.Message
        );
      }

      var listIds = new List<string>() { id };

      await _pub.Publish(Topics.UpdateLogicProc, listIds);

      var ret = CreatedAtAction(nameof(Delete), null, id);
      return ret;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<StaticLogicDTO>> Get(string id)
    {
      var obj = await _logicService.GetListByIdsAsync(new List<string>() { id });

      if (obj is null || obj.Count == 0)
      {
        return NotFound();
      }

      return obj.FirstOrDefault();
    }

    [HttpGet]
    [Route("GetByFigureAsync")]
    public async Task<List<StaticLogicDTO>> GetByFigureAsync(string id)
    {
      var obj = await _logicService.GetByFigureAsync(id);
      return obj;
    }

    [HttpGet]
    [Route("GetByName")]
    public async Task<List<StaticLogicDTO>> GetByName(string name)
    {
      if (User.IsInRole(RoleConstants.admin))
      {
        
      }
      var obj = await _logicService.GetByName(name);
      return obj;
    }
  }
}
