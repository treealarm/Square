using DbLayer;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class MapController : ControllerBase
  {
    private readonly MapService _mapService;

    public MapController(MapService mapsService) =>
        _mapService = mapsService;

    [HttpGet]
    public async Task<List<Marker>> Get()
    {
      var test = await _mapService.GetAsync();
      return test;
    }
        

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Marker>> Get(string id)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      return marker;
    }

    [HttpGet()]
    [Route("GetByParent")]
    public async Task<List<Marker>> GetByParent(string parent_id)
    {
      var test = await _mapService.GetByParentIdAsync(parent_id);
      return test;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Marker newMarker)
    {
      await _mapService.CreateAsync(newMarker);

      var ret = CreatedAtAction(nameof(Get), new { id = newMarker.id }, newMarker);
      return ret;
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Marker updatedMarker)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      updatedMarker.id = marker.id;

      await _mapService.UpdateAsync(id, updatedMarker);

      return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      var marker = await _mapService.GetAsync(id);

      if (marker is null)
      {
        return NotFound();
      }

      await _mapService.RemoveAsync(marker.id);

      var ret = CreatedAtAction(nameof(Delete), new { id = marker.id }, marker);

      return ret;
    }
  }
}