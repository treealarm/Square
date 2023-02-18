using DbLayer.Services;
using Domain;
using Domain.Rights;
using Domain.ServiceInterfaces;
using Domain.States;
using Itinero;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
  
  public class RightsController: ControllerBase
  {
    private readonly IRightService _rightsService;
    public RightsController(
      IRightService rightsService
    )
    {
      _rightsService = rightsService;
    }

    [HttpPost]
    [Route("UpdateRights")]
    public async Task<ActionResult<List<ObjectRightsDTO>>> Update(List<ObjectRightsDTO> newObjs)
    {
      await _rightsService.UpdateListAsync(newObjs);
      return CreatedAtAction(nameof(Update), newObjs);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      try
      {
        await _rightsService.DeleteAsync(id);
      }
      catch (Exception ex)
      {
        return StatusCode(
          StatusCodes.Status500InternalServerError,
          ex.Message
        );
      }

      var listIds = new List<string>() { id };

      var ret = CreatedAtAction(nameof(Delete), null, id);
      return ret;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<ObjectRightsDTO>> Get(string id)
    {
      var obj = await _rightsService.GetListByIdsAsync(new List<string>() { id });

      if (obj is null || obj.Count == 0)
      {
        return NotFound();
      }

      return obj.Values.FirstOrDefault();
    }

    [HttpPost()]
    [Route("GetRights")]
    public async Task<List<ObjectRightsDTO>> GetRights(List<string> ids)
    {
      var data = await _rightsService.GetListByIdsAsync(ids);
      return data.Values.ToList();
    }
  }
}
