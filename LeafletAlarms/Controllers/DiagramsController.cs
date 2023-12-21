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
    [Route("GetDiagramByParent")]
    public async Task<GetDiagramDTO> GetDiagramByParent(string parent_id, int level = 1)
    {
      var retVal = new GetDiagramDTO();

      var markers = await _mapService.GetByParentIdAsync(parent_id, null, null, 1000);
      var children = markers;

      for (int l = 0; l < level; ++l)
      {
        children = await _mapService.GetByParentIdsAsync(children.Values.Select(m => m.id).ToList(), null, null, 1000);
        markers = markers.Union(children).ToDictionary();
      }

      var diagrams = await _diagramService.GetListByIdsAsync(markers.Keys.ToList());
      var props = await _mapService.GetPropsAsync(markers.Keys.ToList());

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
      }

      
      retVal.content = diagrams.Values.ToList();

      var propsParent = await _mapService.GetPropsAsync(new List<string>() { parent_id });
      var parent = propsParent.FirstOrDefault();

      if (parent.Value != null)
      {
        retVal.parent = new DiagramDTO()
        {
          id = parent_id
        };
        retVal.parent.extra_props = propsParent.FirstOrDefault().Value.extra_props;
      }
     
      return retVal;
    }

    [HttpGet()]
    [Route("CreateBasicTemplate")]
    public async Task<List<DiagramDTO>> CreateBasicTemplate()
    {
      var retVal1 = new List<DiagramDTO>();

      var container_diagram = new DiagramDTO()
      {
        id = "111100000000000000000001",
        parent_id = "655f41cfa139722c4f07a7b7",
        extra_props = new List<ObjExtraPropertyDTO>()
        {
          new ObjExtraPropertyDTO()
          {
            prop_name = "__paper_width",
            str_val = "1000"
          }
        }
      };
      retVal1.Add( container_diagram );

      var rack0 = new DiagramDTO()
      {
        id = "111100000000000000000002",
        parent_id = container_diagram.id,
        name = "Name",
        geometry = new DiagramCoordDTO()
        {
          left = 50,
          top = 10,
          width = 200,
          height = 500
        }
      };
      retVal1.Add(rack0 );

      var rack1 = new DiagramDTO()
      {
        id = "111100000000000000000003",
        parent_id = container_diagram.id,
        name = "Name1",
        geometry = new DiagramCoordDTO()
        {
          left = 300,
          top = 10,
          width = 200,
          height = 500
        }
      };
      retVal1.Add(rack1 );

      await _diagramService.UpdateListAsync( retVal1 );
      await _mapService.UpdateHierarchyAsync(retVal1);
      await _mapService.UpdatePropNotDeleteAsync(retVal1);
      return retVal1;
    }
  }
}
