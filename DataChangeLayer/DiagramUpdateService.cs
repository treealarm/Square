
using Domain.Diagram;
using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  public class DiagramUpdateService : IDiagramUpdateService
  {
    private readonly IMapService _mapService;
    private readonly IDiagramService _diagramService;

    private IPubService _pub;
    public DiagramUpdateService(
     IMapService mapsService,
     IDiagramService diagramService,
     IPubService pub
   )
    {
      _mapService = mapsService;
      _diagramService = diagramService;
      _pub = pub;
    }
    public async Task<List<DiagramDTO>> UpdateDiagrams(List<DiagramDTO> dgrs)
    {
      await _mapService.UpdateHierarchyAsync(dgrs);
      await _diagramService.UpdateListAsync(dgrs);

      return dgrs;
    }
  }
}
