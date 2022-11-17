using DbLayer;
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
    public async Task<ActionResult<StaticLogicDTO>> Update(StaticLogicDTO newObj)
    {
      await _logicService.UpdateAsync(newObj);
      return CreatedAtAction(nameof(Update), newObj);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      await _logicService.DeleteAsync(id);
      var ret = CreatedAtAction(nameof(Delete), id);
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
  }
}
