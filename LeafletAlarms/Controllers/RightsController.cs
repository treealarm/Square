using Domain.Rights;
using Domain.ServiceInterfaces;
using Itinero;
using LeafletAlarms.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConstants.admin)]
  
  public class RightsController: ControllerBase
  {
    private readonly IRightService _rightsService;
    private readonly KeyCloakConnectorService _kcService;
    public RightsController(
      IRightService rightsService,
      KeyCloakConnectorService kcService
    )
    {
      _rightsService = rightsService;
      _kcService = kcService;
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

    
    

    [HttpGet()]
    [Route("GetRoles")]
    public async Task<List<string>> GetRoles()
    {
      await _kcService.GetOath2Token();
      return await _kcService.GetRoles();
    }

    [HttpGet()]
    [Route("GetRightValues")]
    public List<RightValuesDTO> GetRightValues()
    {
      var ret = new List<RightValuesDTO>();

      foreach (var right in Enum.GetValues<ObjectRightValueDTO.ERightValue>())
      {
        if (right == ObjectRightValueDTO.ERightValue.None)
        {
          continue;
        }

        var element = new RightValuesDTO()
        {
          RightName = right.ToString(),
          RightValue = (int)right
        };
        ret.Add(element);
      }
      return ret;
    }
  }
}
