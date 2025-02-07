
using Domain;

namespace DataChangeLayer
{
  internal class DiagramTypeUpdateService : IDiagramTypeUpdateService
  {
    private readonly IDiagramTypeService _diagramTypeService;
    public DiagramTypeUpdateService(
      IDiagramTypeService diagramTypeService
    )
    {
      _diagramTypeService = diagramTypeService;
    }
    public async Task<List<string>> DeleteDiagramTypes(List<string> ids)
    {
      await _diagramTypeService.DeleteAsync(ids);
      return ids;
    }

    public async Task<GetDiagramTypesDTO> UpdateDiagramTypes(List<DiagramTypeDTO> dgr_types)
    {
      await _diagramTypeService.UpdateListAsync(dgr_types);

      var retVal = new GetDiagramTypesDTO(dgr_types);
      return retVal;
    }
  }
}
