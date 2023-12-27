using Domain.Diagram;
using Domain;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DbLayer.Services;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DiagramTypesController : ControllerBase
  {
    private readonly IDiagramTypeService _diagramTypeService;
    public DiagramTypesController(
      IDiagramTypeService diagramTypeService
    )
    {
      _diagramTypeService = diagramTypeService;
    }

    [HttpGet()]
    [Route("GetDiagramTypes")]
    public async Task<List<DiagramTypeDTO>> GetDiagramTypes([FromQuery] List<string> diagram_ids)
    {
      var dic = await _diagramTypeService.GetListByTypeNamesAsync( diagram_ids );
      return dic.Values.ToList();
    }

    [HttpGet()]
    [Route("CreateBasicTemplate")]
    public async Task<List<DiagramTypeDTO>> CreateBasicTemplate()
    {
      List<DiagramTypeDTO> list = new List<DiagramTypeDTO>();

      for (int i = 0; i < 2; i++)
      {
        var diagramType = new DiagramTypeDTO()
        {
          id = i == 0 ? "6582fbd7203a7d44110c8d1d": "6582fbd7203a7d44110c8d1e",
          src = "svg/rack.svg",
          name = $"rack{i}",
          regions = new List<DiagramTypeRegionDTO>()
        };

        diagramType.regions.Add(new DiagramTypeRegionDTO()
        {
          id = "1",
          geometry = new DiagramCoordDTO()
          {
            top = 0.5,
            left = 0.155,
            height = 0.05,
            width = 0.8
          }
        });

        diagramType.regions.Add(new DiagramTypeRegionDTO()
        {
          id = "2",
          geometry = new DiagramCoordDTO()
          {
            top = 0.7,
            left = 0.155,
            height = 0.05,
            width = 0.8
          }
        });

        list.Add(diagramType);
      }

      var diagramTypeDevice = new DiagramTypeDTO()
      {
        id = "6582fbd7203a7d44110c8d1f",
        src = "svg/cisco.svg",
        name = $"cisco",
      };
      list.Add(diagramTypeDevice);

      await _diagramTypeService.UpdateListAsync(list);
      return list;
    }
  }
}
