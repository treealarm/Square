using Domain.Integration;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class IntegrationController : ControllerBase
  {
    private readonly IIntegrationService _integrationService;
    private readonly IIntegrationUpdateService _integrationUpdateService;

    public IntegrationController(
     IIntegrationService integrationService,
     IIntegrationUpdateService integrationUpdateService
    )
    {
      _integrationService = integrationService;
      _integrationUpdateService = integrationUpdateService;
    }

    [HttpPost]
    [Route("Update")]
    public async Task<IActionResult> Update(List<IntegrationDTO> obj2UpdateIn)
    {
      try
      {
        await _integrationUpdateService.UpdateListAsync(obj2UpdateIn);
      }
      
      catch (Exception ex)
      {
        return StatusCode(500, new { Message = "An error occurred while processing request", Detailed = ex.Message });
      }
      return Ok();
    }
    [HttpDelete]
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
    public async Task<Dictionary<string, IntegrationDTO>> GetByParentIdsAsync(
       string parent_id,
       string start_id,
       string end_id,
       int count
     )
    {
      var ret_val = await _integrationService.GetByParentIdsAsync(new List<string>() { parent_id }, start_id, end_id, count);
      return ret_val;
    }
  }
}
