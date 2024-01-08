using DbLayer.Services;
using Domain;
using Domain.Diagram;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using LeafletAlarms.Services;
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
    private readonly TracksUpdateService _trackUpdateService;

    public DiagramsController(
     IDiagramTypeService diagramTypeService,
     IDiagramService diagramService,
     IMapService mapService,
     TracksUpdateService trackUpdateService
    )
    {
      _diagramTypeService = diagramTypeService;
      _diagramService = diagramService;
      _mapService = mapService;
      _trackUpdateService = trackUpdateService;
    }

    [HttpGet()]
    [Route("GetDiagramByParent")]
    public async Task<GetDiagramDTO> GetDiagramByParent(string parent_id, int depth = 1)
    {
      var retVal = new GetDiagramDTO()
      {
        parent_id = parent_id,
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

    private static FiguresDTO GetDiaFig()
    {
      var figs = new FiguresDTO();
      var fig = new FigureGeoDTO();
      figs.figs.Add(fig);

      fig.id = "222200000000000000000001";
      fig.name = "Diagram";
      var geo = new GeometryPolygonDTO();

      fig.geometry = geo;
      fig.geometry.type = "Polygon";

      fig.extra_props = new List<ObjExtraPropertyDTO>()
      {
        new ObjExtraPropertyDTO()
        {
          prop_name = "__paper_width",
          str_val = "1000"
        },
        new ObjExtraPropertyDTO()
        {
          prop_name = "__paper_height",
          str_val = "1000"
        }
        ,
        new ObjExtraPropertyDTO()
        {
          prop_name = "__is_diagram",
          str_val = "1"
        }
      };

      geo.coord.Add(new Geo2DCoordDTO()
      {
        Lat = 55.7566737398449,
        Lon = 37.60722931951715
      });

      geo.coord.Add(new Geo2DCoordDTO()
      {
        Lat = 55.748852242908995,
        Lon = 37.60259563134112
      });

      geo.coord.Add(new Geo2DCoordDTO()
      {
        Lat = 55.75203896803514,
        Lon = 37.618727730916895
      });

      return figs;
    }

    [HttpGet()]
    [Route("CreateBasicTemplate")]
    public async Task<List<DiagramDTO>> CreateBasicTemplate()
    {
      var retVal1 = new List<DiagramDTO>();

      var figs = GetDiaFig();
      var container_diagram = GetDiaFig().figs.First();
      await _trackUpdateService.UpdateFigures(figs);

      var rack0 = new DiagramDTO()
      {
        id = "222200000000000000000002",
        parent_id = container_diagram.id,
        name = "Name",
        dgr_type = "rack0",
        geometry = new DiagramCoordDTO()
        {
          left = 50,
          top = 10,
          width = 200,
          height = 500
        }
      };

      rack0.extra_props = new List<ObjExtraPropertyDTO>()
      {
        new ObjExtraPropertyDTO()
        {
          prop_name = "__paper_width",
          str_val = "1000"
        },
        new ObjExtraPropertyDTO()
        {
          prop_name = "__paper_height",
          str_val = "1000"
        }
        ,
        new ObjExtraPropertyDTO()
        {
          prop_name = "__is_diagram",
          str_val = "1"
        }
      };

      retVal1.Add(rack0 );

      var rack1 = new DiagramDTO()
      {
        id = "222200000000000000000003",
        parent_id = container_diagram.id,
        name = "Name1",
        dgr_type = "rack1",
        geometry = new DiagramCoordDTO()
        {
          left = 300,
          top = 10,
          width = 200,
          height = 500
        }
      };
      retVal1.Add(rack1 );

      var cisco1 = new DiagramDTO()
      {
        id = "222200000000000000000004",
        parent_id = rack0.id,
        name = "Cisco1",
        dgr_type = "cisco",
        region_id = "1",
      };
      retVal1.Add(cisco1);

      var cisco2 = new DiagramDTO()
      {
        id = "222200000000000000000005",
        parent_id = rack0.id,
        name = "Cisco2",
        dgr_type = "cisco",
        region_id = "2",
      };
      retVal1.Add(cisco2);

      var cisco3 = new DiagramDTO()
      {
        id = "222200000000000000000006",
        parent_id = rack1.id,
        name = "Cisco3",
        dgr_type = "cisco",
        region_id = "2",
      };
      retVal1.Add(cisco3);

      await _diagramService.UpdateListAsync( retVal1 );
      await _mapService.UpdateHierarchyAsync(retVal1);
      await _mapService.UpdatePropNotDeleteAsync(retVal1);
      return retVal1;
    }
  }
}
