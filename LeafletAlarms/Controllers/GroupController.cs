using Domain;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class GroupController : ControllerBase
  {
    private readonly IGroupUpdateService _groupUpdateService;
    private readonly IGroupsService _groupService;
    public GroupController(
      IGroupUpdateService groupUpdateService,
      IGroupsService groupService
    )
    {
      _groupUpdateService = groupUpdateService;
      _groupService = groupService;
    }

    [HttpPost()]
    [Route("GetByNames")]
    public async Task<List<GroupDTO>> GetByNames(List<string> names)
    {
      var dic = await _groupService.GetListByNamesAsync(names);
      return dic.Values.ToList();
    }

    [HttpPost]
    [Route("UpdateGroups")]
    public async Task<List<GroupDTO>> UpdateValues([FromBody] List<GroupDTO> objects)
    {
      await _groupUpdateService.UpdateListAsync(objects);
      return objects;
    }

    [HttpDelete()]
    [Route("DeleteGroups")]
    public async Task DeleteGroups(List<string> ids)
    {
      await _groupUpdateService.RemoveAsync(ids);
    }
    [HttpDelete()]
    [Route("DeleteGroupsByNames")]
    public async Task DeleteGroupsByNames(List<string> names)
    {
      await _groupUpdateService.RemoveByNameAsync(names);
    }
  }
}
