using Domain.Integration;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class IntegrationLeafsController : ControllerBase
  {
    private readonly IIntegrationLeafsService _integrationService;
    private readonly IIntegrationLeafsUpdateService _integrationUpdateService;

    public IntegrationLeafsController(
     IIntegrationLeafsService integrationService,
     IIntegrationLeafsUpdateService integrationUpdateService
    )
    {
      _integrationService = integrationService;
      _integrationUpdateService = integrationUpdateService;
    }

    [HttpPost]
    [Route("Update")]
    public async Task<ActionResult<List<IntegrationLeafDTO>>> Update(List<IntegrationLeafDTO> obj2UpdateIn)
    {
      try
      {
        await _integrationUpdateService.UpdateListAsync(obj2UpdateIn);
      }
      
      catch (Exception ex)
      {
        return StatusCode(500, new { Message = "An error occurred while processing request", Detailed = ex.Message });
      }
      return Ok(obj2UpdateIn);
    }

    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> Delete(List<string> ids)
    {      
      try
      {
        await _integrationUpdateService.RemoveAsync(ids);
      }

      catch (Exception ex)
      {
        return StatusCode(500, new { Message = "An error occurred while processing request", Detailed = ex.Message });
      }
      return Ok();
    }
    [HttpGet()]
    [Route("GetByParent")]
    public async Task<GetIntegrationLeafsDTO> GetByParentAsync(
       string integration_id,
       int count
     )
    {
      var ret = new GetIntegrationLeafsDTO()
      {
        integration_id = integration_id
      };

      var ret_val = await _integrationService.GetByParentIdsAsync(new List<string>() { integration_id });

      ret.children = ret_val.Values.ToList();
      return ret;
    }
  }
}
