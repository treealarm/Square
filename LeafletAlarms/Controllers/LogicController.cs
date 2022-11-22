using DbLayer;
using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LogicController : ControllerBase
  {
    private readonly ILogicService _logicService;
    public LogicController(
      ILogicService logicService
    )
    {
      _logicService = logicService;
    }

    [HttpPost]
    [Route("UpdateLogic")]
    public async Task<ActionResult<List<StaticLogicDTO>>> Update(List<StaticLogicDTO> newObjs)
    {
      foreach (var newObj in newObjs)
      {
        await _logicService.UpdateAsync(newObj);
      }
      
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

      var ret = CreatedAtAction(nameof(Delete), null, id);
      return ret;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<StaticLogicDTO>> Get(string id)
    {
      var obj = await _logicService.GetAsync(id);

      if (obj is null)
      {
        return NotFound();
      }

      return obj;
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
      var obj = await _logicService.GetByName(name);
      return obj;
    }
  }
}
