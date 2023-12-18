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
    public async Task<GetDiagramDTO> GetDiagram(
      string diagram_id
    )
    {
      await Task.Delay(0);
      var retVal = new GetDiagramDTO()
      {
        container_diagram = new DiagramDTO() 
        { 
          id = "655f41cfa139722c4f07a7b7" ,
          extra_props = new List<ObjExtraPropertyDTO>()
          {
            new ObjExtraPropertyDTO()
            {
              prop_name = "__paper_width",
              str_val = "500"
            }
          }
        },

        content = new List<DiagramDTO>()
        {
          new DiagramDTO()
          {
            id = "111100000000000000000002",
            name = "Name",
            geometry = new DiagramCoordDTO()
            {
              left = 10,
              top = 10,
              width = 20,
              height = 50
            }
          },
          new DiagramDTO()
          {
            id = "111100000000000000000003",
            name = "Name1",
            geometry = new DiagramCoordDTO()
            {
              left = 40,
              top = 10,
              width = 20,
              height = 50
            }
          }
        }
      };     

      return retVal;
    }
  }
}
