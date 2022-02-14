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
      var book = await _mapService.GetAsync(id);

      if (book is null)
      {
        return NotFound();
      }

      return book;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Marker newBook)
    {
      await _mapService.CreateAsync(newBook);

      return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Marker updatedBook)
    {
      var book = await _mapService.GetAsync(id);

      if (book is null)
      {
        return NotFound();
      }

      updatedBook.Id = book.Id;

      await _mapService.UpdateAsync(id, updatedBook);

      return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      var book = await _mapService.GetAsync(id);

      if (book is null)
      {
        return NotFound();
      }

      await _mapService.RemoveAsync(book.Id);

      return NoContent();
    }
  }
}