using DbLayer.Services;
using Domain;
using Domain.Diagram;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DiagramsController : ControllerBase
  {
    private readonly IDiagramTypeService _diagramTypeService;

    public DiagramsController(
     IDiagramTypeService diagramTypeService
    )
    {
      _diagramTypeService = diagramTypeService;
    }

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
              str_val = "1000"
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
              left = 50,
              top = 10,
              width = 200,
              height = 500
            }
          },
          new DiagramDTO()
          {
            id = "111100000000000000000003",
            name = "Name1",
            geometry = new DiagramCoordDTO()
            {
              left = 300,
              top = 10,
              width = 200,
              height = 500
            }
          }
        }
      };     

      return retVal;
    }
  }
}
