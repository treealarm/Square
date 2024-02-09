using Domain.Diagram;
using Domain;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DbLayer.Services;
using Domain.DiagramType;

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

    [HttpPost()]
    [Route("GetDiagramTypesByName")]
    public async Task<GetDiagramTypesDTO> GetDiagramTypesByName(List<string> typeNames)
    {      
      var dic = await _diagramTypeService.GetListByTypeNamesAsync( typeNames );
      var retVal = new GetDiagramTypesDTO(dic.Values.ToList());
      return retVal;
    }

    [HttpPost()]
    [Route("GetDiagramTypesById")]
    public async Task<GetDiagramTypesDTO> GetDiagramTypesById(List<string> ids)
    {
      var dic = await _diagramTypeService.GetListByTypeIdsAsync(ids);
      var retVal = new GetDiagramTypesDTO(dic.Values.ToList());
      return retVal;
    }

    [HttpDelete()]
    [Route("DeleteDiagramTypes")]
    public async Task<List<string>> DeleteDiagramTypes(List<string> ids)
    {
      await _diagramTypeService.DeleteAsync(ids);
      return ids;
    }

    [HttpPost()]
    [Route("GetDiagramTypesByFilter")]
    public async Task<GetDiagramTypesDTO> GetDiagramTypesByFilter(GetDiagramTypesByFilterDTO filter)
    {
      var dic = await _diagramTypeService.GetDiagramTypesByFilter(filter);
      var retVal = new GetDiagramTypesDTO(dic.Values.ToList());
      return retVal;
    }

    [HttpPost()]
    [Route("UpdateDiagramTypes")]
    public async Task<GetDiagramTypesDTO> UpdateDiagramTypes(List<DiagramTypeDTO> dgr_types)
    {
      await _diagramTypeService.UpdateListAsync(dgr_types);

      var retVal = new GetDiagramTypesDTO(dgr_types);
      return retVal;
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
        regions = new List<DiagramTypeRegionDTO>()
      };

      diagramTypeDevice.regions.Add(new DiagramTypeRegionDTO()
      {
        id = "port1",
        geometry = new DiagramCoordDTO()
        {
          top = 0.05,
          left = 0.04,
          height = 0.3,
          width = 0.05
        }
      });

      diagramTypeDevice.regions.Add(new DiagramTypeRegionDTO()
      {
        id = "port2",
        geometry = new DiagramCoordDTO()
        {
          top = 0.4,
          left = 0.04,
          height = 0.3,
          width = 0.05
        }
      });

      list.Add(diagramTypeDevice);

      var diagramTypePort = new DiagramTypeDTO()
      {
        id = "2222fbd7203a7d44110c8d1f",
        src = "svg/port.svg",
        name = $"port",
        regions = new List<DiagramTypeRegionDTO>()
      };
      list.Add(diagramTypePort);

      await _diagramTypeService.UpdateListAsync(list);
      return list;
    }
  }
}
