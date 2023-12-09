using Domain;
using Domain.Diagram;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DiagramsController : ControllerBase
  {
    [HttpGet()]
    [Route("GetDiagram")]
    public async Task<DiagramDTO> GetDiagram(
      string diagram_id
    )
    {
      await Task.Delay(0);
      var retVal = new DiagramDTO();     

      return retVal;
    }
  }
}
