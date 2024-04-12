using Domain;
using Domain.Diagram;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Pipelines.Sockets.Unofficial.Arenas;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DiagramsController : ControllerBase
  {
    private readonly IDiagramTypeService _diagramTypeService;
    private readonly IDiagramService _diagramService;
    private readonly IMapService _mapService;    

    public DiagramsController(
     IDiagramTypeService diagramTypeService,
     IDiagramService diagramService,
     IMapService mapService
    )
    {
      _diagramTypeService = diagramTypeService;
      _diagramService = diagramService;
      _mapService = mapService;
    }

    [HttpGet()]
    [Route("GetDiagramById")]
    public async Task<DiagramDTO> GetDiagramById(string diagram_id)
    {
      var retVal = new DiagramDTO();

      if (string.IsNullOrEmpty(diagram_id))
      {
        return retVal;
      }

      var diagrams = await _diagramService.GetListByIdsAsync(new List<string>() { diagram_id });

      if (diagrams.TryGetValue(diagram_id, out var value))
      {
        retVal = value;
      }
      var marker = await _mapService.GetAsync(diagram_id);    

      var props = await _mapService.GetPropsAsync(new List<string>() { diagram_id });
      retVal.id = marker.id;
      retVal.name = marker.name;
      retVal.parent_id = marker.parent_id;
      retVal.external_type = marker.external_type;

      if (props.TryGetValue(diagram_id, out var valueProps))
      {
        retVal.extra_props = valueProps.extra_props;
      }     

      return retVal;
    }

    [HttpGet()]
    [Route("GetDiagramByParent")]
    public async Task<GetDiagramDTO> GetDiagramByParent(string parent_id, int depth = 1)
    {
      var retVal = new GetDiagramDTO()
      {
        depth = depth
      };

      if (string.IsNullOrEmpty(parent_id))
      {
        return retVal;
      }

      var markerParent = await _mapService.GetAsync(new List<string>() { parent_id });

      var markers = await _mapService.GetByParentIdAsync(parent_id, null, null, 1000);
      var children = markers;

      for (int l = 0; l < depth; ++l)
      {
        if (!children.Any())
        { continue; }
        children = await _mapService.GetByParentIdsAsync(children.Values.Select(m => m.id).ToList(), null, null, 1000);
        markers = markers.Union(children).ToDictionary();
      }
      markers[parent_id] = markerParent[parent_id];

      var diagrams = await _diagramService.GetListByIdsAsync(markers.Keys.ToList());

      var props = await _mapService.GetPropsAsync(markers.Keys.ToList());

      HashSet<string> dgrTypes = new HashSet<string>();

      foreach (var kvp in diagrams)
      {
        if (props.TryGetValue(kvp.Key, out var value))
        {
          kvp.Value.extra_props = value.extra_props;
        }

        if (markers.TryGetValue(kvp.Key, out var marker))
        {
          kvp.Value.name = marker.name;
          kvp.Value.parent_id = marker.parent_id;
          kvp.Value.external_type = marker.external_type;
        }

        if (!string.IsNullOrEmpty(kvp.Value.dgr_type))
        {
          dgrTypes.Add(kvp.Value.dgr_type);
        }
      }

      if (!diagrams.TryGetValue(parent_id, out var diagram))
      {
        var parentMarker = markerParent[parent_id];
        retVal.parent = new DiagramDTO()
        {
          id = parentMarker.id,
          parent_id = parentMarker.parent_id,
          name = parentMarker.name,
          external_type = parentMarker.external_type,
        };

        if (props.TryGetValue(parentMarker.id, out var value))
        {
          retVal.parent.extra_props = value.extra_props;
        }
      }
      else
      {
        retVal.parent = diagrams[parent_id];
      }

      retVal.content = diagrams.Values.ToList();

      if (dgrTypes.Count > 0)
      {
        var dgr_types = await _diagramTypeService.GetListByTypeNamesAsync(dgrTypes.ToList());
        retVal.dgr_types = dgr_types.Values.ToList();
      }

      // Fill out parents.
      var parents = await _mapService.GetByChildIdAsync(parent_id);

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
      await _mapService.UpdateHierarchyAsync(dgrs);
      await _diagramService.UpdateListAsync(dgrs);
      //await _mapService.UpdatePropAsync(dgrs);
      return dgrs;
    }
  }
}
