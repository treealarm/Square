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
    private readonly IDiagramService _diagramService;
    private readonly IMapService _mapService;
    private readonly IDiagramUpdateService _diagramUpdateService;

    public DiagramsController(
     IDiagramTypeService diagramTypeService,
     IDiagramService diagramService,
     IMapService mapService,
     IDiagramUpdateService diagramUpdateService
    )
    {
      _diagramTypeService = diagramTypeService;
      _diagramService = diagramService;
      _mapService = mapService;
      _diagramUpdateService = diagramUpdateService;
    }

    [HttpGet()]
    [Route("GetDiagramById")]
    public async Task<DiagramDTO> GetDiagramById(string diagram_id)
    { 
      if (string.IsNullOrEmpty(diagram_id))
      {
        return null;
      }

      var diagrams = await _diagramService.GetListByIdsAsync(new List<string>() { diagram_id });

      if (diagrams.TryGetValue(diagram_id, out var value))
      {
        return value;
      }  

      return null;
    }

    
    [HttpGet()]
    [Route("GetDiagramFull")]
    public async Task<DiagramFullDTO> GetDiagramFull(string diagram_id)
    {
      if (string.IsNullOrEmpty(diagram_id))
      {
        return null;
      }

      var marker = await _mapService.GetAsync(diagram_id);
      
      var listOfIds = new List<string>() { diagram_id };

      if (!string.IsNullOrEmpty(marker.parent_id))
      {
        listOfIds.Add(marker.parent_id);
      }
     
      var diagrams = await _diagramService.GetListByIdsAsync(listOfIds);

      if (diagrams.TryGetValue(diagram_id, out var value))
      {        
        var retVal = new DiagramFullDTO()
        {
          diagram = value
        };

        if (!string.IsNullOrEmpty(marker.parent_id) && diagrams.TryGetValue(marker.parent_id, out var parent_diagram))
        {
          var dgr_types = await _diagramTypeService.GetListByTypeNamesAsync(
            new List<string>()
              {parent_diagram.dgr_type}
            );
          retVal.parent_type = dgr_types.Values.FirstOrDefault();
        }
        return retVal;
      }

      return null;
    }

    [HttpGet()]
    [Route("GetDiagramContent")]
    public async Task<DiagramContentDTO> GetDiagramContent(string diagram_id, int depth = 1)
    {
      var retVal = new DiagramContentDTO()
      {
        depth = depth,
        diagram_id = diagram_id
      };

      if (string.IsNullOrEmpty(diagram_id))
      {
        return retVal;
      }

      var markerParent = await _mapService.GetAsync(new List<string>() { diagram_id });

      Dictionary<string, BaseMarkerDTO> markers = new Dictionary<string, BaseMarkerDTO>();

      if (depth > 0)
      {
        markers = await _mapService.GetByParentIdAsync(diagram_id, null, null, 1000);
        var children = markers;

        for (int l = 0; l < depth; ++l)
        {
          if (!children.Any())
          {
            break;
          }
          children = await _mapService.GetByParentIdsAsync(children.Values.Select(m => m.id).ToList(), null, null, 1000);
          markers = markers.Union(children).ToDictionary();
        }
      }
      markers[diagram_id] = markerParent[diagram_id];

      var i_diagrams = await _diagramService.GetListByIdsAsync(markers.Keys.ToList());

      Dictionary<string, DiagramDTO> diagrams = i_diagrams
          .ToDictionary(
              kvp => kvp.Key,
              kvp => new DiagramDTO
              {
                id = kvp.Value.id,
                geometry = kvp.Value.geometry,
                region_id = kvp.Value.region_id,
                dgr_type = kvp.Value.dgr_type,
                background_img = kvp.Value.background_img
              });



      HashSet<string> dgrTypes = new HashSet<string>();

      retVal.children = markers.Values.ToList();

      foreach (var kvp in diagrams)
      {
        if (!string.IsNullOrEmpty(kvp.Value.dgr_type))
        {
          dgrTypes.Add(kvp.Value.dgr_type);
        }
      }

      retVal.content = diagrams.Values.ToList();

      if (dgrTypes.Count > 0)
      {
        var dgr_types = await _diagramTypeService.GetListByTypeNamesAsync(dgrTypes.ToList());
        retVal.dgr_types = dgr_types.Values.ToList();
      }

      // Fill out parents.
      var parents = await _mapService.GetByChildIdAsync(diagram_id);

      retVal.parents = new List<BaseMarkerDTO>();

      foreach (var parent in parents)
      {
        retVal.parents.Insert(0, parent);
      }

      return retVal;
    }

    [HttpPost()]
    [Route("UpdateDiagrams")]
    public async Task<List<DiagramDTO>> UpdateDiagrams(List<DiagramDTO> dgrs)
    {
      dgrs = dgrs.Where(d => !string.IsNullOrEmpty(d.id)).ToList();
      return await _diagramUpdateService.UpdateDiagrams(dgrs);
    }

    [HttpDelete()]
    [Route("DeleteDiagrams")]
    public async Task<List<string>> DeleteDiagrams(List<string> dgrs)
    {
      return await _diagramUpdateService.DeleteDiagrams(dgrs);
    }
    
  }
}
