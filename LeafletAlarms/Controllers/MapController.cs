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

    public MapController(MapService booksService) =>
        _mapService = booksService;

    [HttpGet]
    public async Task<List<CircleMarker>> Get() =>
        await _mapService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<CircleMarker>> Get(string id)
    {
      var book = await _mapService.GetAsync(id);

      if (book is null)
      {
        return NotFound();
      }

      return book;
    }

    [HttpPost]
    public async Task<IActionResult> Post(CircleMarker newBook)
    {
      await _mapService.CreateAsync(newBook);

      return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, CircleMarker updatedBook)
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