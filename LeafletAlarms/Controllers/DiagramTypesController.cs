using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Domain.DiagramType;

namespace LeafletAlarms.Controllers
{
    [Route("api/[controller]")]
  [ApiController]
  public class DiagramTypesController : ControllerBase
  {
    private readonly IDiagramTypeService _diagramTypeService;
    private readonly IDiagramTypeUpdateService _diagramTypeUpdateService;
    public DiagramTypesController(
      IDiagramTypeService diagramTypeService,
      IDiagramTypeUpdateService diagramTypeUpdateService
    )
    {
      _diagramTypeService = diagramTypeService;
      _diagramTypeUpdateService = diagramTypeUpdateService;
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

    [HttpPost()]
    [Route("GetDiagramTypesByFilter")]
    public async Task<GetDiagramTypesDTO> GetDiagramTypesByFilter(GetDiagramTypesByFilterDTO filter)
    {
      var dic = await _diagramTypeService.GetDiagramTypesByFilter(filter);
      var retVal = new GetDiagramTypesDTO(dic.Values.ToList());
      return retVal;
    }

    [HttpDelete()]
    [Route("DeleteDiagramTypes")]
    public async Task<List<string>> DeleteDiagramTypes(List<string> ids)
    {
      return await _diagramTypeUpdateService.DeleteDiagramTypes(ids);
    }

    [HttpPost()]
    [Route("UpdateDiagramTypes")]
    public async Task<GetDiagramTypesDTO> UpdateDiagramTypes(List<DiagramTypeDTO> dgr_types)
    {
      return await _diagramTypeUpdateService.UpdateDiagramTypes(dgr_types);
    }
  }
}
