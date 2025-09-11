using Domain;
using LeafletAlarms.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize(AuthenticationSchemes = "Bearer", Roles = RoleConstants.admin)]
  
  public class RightsController: ControllerBase
  {
    private readonly IRightService _rightsService;
    private readonly IRightUpdateService _rightUpdateService;
    private readonly KeyCloakConnectorService _kcService;
    public RightsController(
      IRightService rightsService,
      KeyCloakConnectorService kcService,
      IRightUpdateService rightUpdateService
    )
    {
      _rightsService = rightsService;
      _kcService = kcService;
      _rightUpdateService = rightUpdateService;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<List<ObjectRightValueDTO>>> Get(string id)
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
    public async Task<List<ObjectRightValueDTO>> GetRights(List<string> ids)
    {
      var data = await _rightsService.GetListByIdsAsync(ids);
      return data.Values.SelectMany(list => list).ToList();
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

    [HttpPost]
    [Route("UpdateRights")]
    public async Task<ActionResult<List<ObjectRightValueDTO>>> Update(List<ObjectRightValueDTO> newObjs)
    {
      await _rightUpdateService.Update(newObjs);
      return CreatedAtAction(nameof(Update), newObjs);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      try
      {
        await _rightUpdateService.Delete(id);
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
  }
}
