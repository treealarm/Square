using Domain.Integration;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class IntegrationsController : ControllerBase
  {
    private readonly IIntegrationService _integrationService;
    private readonly IIntegrationUpdateService _integrationUpdateService;

    public IntegrationsController(
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
    public async Task<GetIntegrationsDTO> GetByParentAsync(
       string parent_id,
       string start_id,
       string end_id,
       int count
     )
    {
      var ret_val = await _integrationService.GetByParentIdsAsync(new List<string>() { parent_id }, start_id, end_id, count);

      var children_checker = await _integrationService.GetHasChildrenAsync(ret_val.Keys.ToList());

      var ret = new GetIntegrationsDTO()
      {
        parent_id = string.IsNullOrEmpty(parent_id) ? "" : parent_id,
        children = new List<IntegrationExDTO>()
      };

      foreach (var item in ret_val.Values)
      {
        ret.children.Add(new IntegrationExDTO() {
        id = item.id,
        parent_id = item.parent_id,
        name = item.name,
        has_children = children_checker[item.id] == true
        });
      }
      return ret;
    }
  }
}
