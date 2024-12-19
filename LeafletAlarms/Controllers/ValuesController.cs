using Domain;
using Domain.ServiceInterfaces;
using Domain.Values;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{

  [Route("api/[controller]")]
  [ApiController]
  public class ValuesController : ControllerBase  
  {
    private readonly IValuesService _valuesService;
    private readonly IValuesUpdateService _valuesUpdateService;
    private readonly IMapService _mapService;
    public ValuesController(
      IMapService mapService,
      IValuesService valuesService,
      IValuesUpdateService valuesUpdateService
    )
    {
      _mapService = mapService;
      _valuesService = valuesService;
      _valuesUpdateService = valuesUpdateService;
    }

    [HttpPost()]
    [Route("GetByOwners")]
    public async Task<List<ValueDTO>> GetByOwners(List<string> ids)
    {
      var owners_and_views = await _mapService.GetOwnersAndViewsAsync(ids);

      var owners = owners_and_views.Where(i => string.IsNullOrEmpty(i.Value.owner_id)).ToDictionary();

      var data = await _valuesService.GetListByOwnersAsync(owners.Keys.ToList());

      var retval = new List<ValueDTO>();

      foreach (var id in ids)
      {
        if (owners_and_views.TryGetValue(id, out var view))
        {
          var owner_object_id = id;

          if (!string.IsNullOrEmpty(view.owner_id))
          {
            owner_object_id = view.owner_id;
          }
          var states = data.Values.Where(d => d.owner_id == owner_object_id).ToList();

          if (!states.Any())
          {
            continue;
          }
          if (!string.IsNullOrEmpty(view.owner_id))
          {
            foreach(var state in states)
            {
              var state_new = PropertyCopy.CloneObject(state);
              state_new.owner_id = id;
              retval.Add(state_new);
            }              
          }
          else
          {
            retval.AddRange(states);
          }
        }
      }
      return retval;
    }

    [HttpPost()]
    [Route("GetByIds")]
    public async Task<List<ValueDTO>> GetByIds(List<string> ids)
    {
      var dic = await _valuesService.GetListByIdsAsync(ids);
      return dic.Values.ToList();
    }

    [HttpPost]
    [Route("UpdateValues")]
    public async Task UpdateValues([FromBody] List<ValueDTO> values)
    {
      await _valuesUpdateService.UpdateValues(values);
    }

    [HttpPost]
    [Route("UpdateValuesFilteredByName")]
    public async Task<List<ValueDTO>> UpdateValuesFilteredByName(List<ValueDTO> values)
    {
      var dicUpdated =  await _valuesUpdateService.UpdateValuesFilteredByNameAsync(values);
      return dicUpdated.Values.ToList();
    }

    [HttpDelete()]
    [Route("DeleteValues")]
    public async Task DeleteValues(List<string> ids)
    {
      await _valuesUpdateService.RemoveValues(ids);
    }
  }
}
