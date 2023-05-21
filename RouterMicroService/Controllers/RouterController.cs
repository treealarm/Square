using Dapr.Client;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace RouterMicroService.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RouterController : ControllerBase
  {
    private readonly DaprClient _daprClient;
    private ITrackRouter _router;
    private IMapService _mapService;
    private IGeoService _geoService;
    public RouterController(
      DaprClient daprClient,
      ITrackRouter router,
      IMapService mapService,
      IGeoService geoService
    )
    {
      _router = router;
      _daprClient = daprClient;
      _mapService = mapService;
      _geoService = geoService;
    }


    [HttpGet()]
    [Route("GetHello")]
    public async Task<IEnumerable<string>> GetHello()
    {
      var forecasts = await _daprClient.InvokeMethodAsync<IEnumerable<string>>(
            HttpMethod.Get,
            @"leafletalarms",
            @"api/Tracks/GetHello");

      return forecasts;
    }

    [HttpPost]
    [Route("GetRoute")]
    public async Task<ActionResult<List<Geo2DCoordDTO>>> GetRoute(RoutDTO routData)
    {
      var routRet = await _router.GetRoute(
        routData.InstanceName,
        routData.Profile,
        routData.Coordinates
        );
      return CreatedAtAction(nameof(GetRoute), routRet);
    }

    private async Task<Dictionary<string, GeoObjectDTO>> GetNearestParkings(
      Geo2DCoordDTO myPoint, 
      int parkings 
      )
    {
      ObjPropsSearchDTO property_filter = new ObjPropsSearchDTO()
      {
        props = new List<KeyValueDTO>()
        {
          new KeyValueDTO()
          {
            prop_name = "layer_name",
            str_val = "parkings"
          }
        }
      };

      var props = await _mapService.GetPropByValuesAsync(
        property_filter,
        null,
        true,
        10000
      );

      var ids = props.Select(i => i.id).ToList();

      var geoParks0 = await _geoService.GetGeoObjectNearestsAsync(
        ids,
        myPoint,
        2
      );

      return geoParks0;
    }
    public static bool IsPointInPolygon4(List<Geo2DCoordDTO> polygon, Geo2DCoordDTO testPoint)
    {
      bool result = false;
      int j = polygon.Count() - 1;
      for (int i = 0; i < polygon.Count(); i++)
      {
        if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y ||
          polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
        {
          if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
          {
            result = !result;
          }
        }
        j = i;
      }
      return result;
    }

    private void GetAlternatives(
      TreeEdgeDTO startPoint,
      string profileName,
      string instanceName,
      int maxPoints,
      HashSet<TreeEdgeDTO> result,
      HashSet<TreeEdgeDTO> forbiddenEdges,
      Dictionary<string, GeoObjectDTO> forbiddenZones
    )
    {
      if (result.Count > maxPoints)
      {
        return;
      }

      List<TreeEdgeDTO> alternatives = null;

      if (startPoint != null)
      {
        alternatives = _router.CalculateTree(instanceName, profileName, startPoint.Shape, 100);
      }

      if (alternatives == null || alternatives.Count == 0)
      {
        return;
      }

      var newAlternatives = new HashSet<TreeEdgeDTO>();

      foreach (var alternative in alternatives)
      {
        if (result.Contains(alternative))
        {
          continue;
        }

        if (IsIntersectedForbidden(alternative, forbiddenZones))
        {
          forbiddenEdges.Add(alternative);
          continue;
        }

        newAlternatives.Add(alternative);
      }

      startPoint.Children = newAlternatives;

      result.UnionWith(alternatives);

      foreach (var alternative in newAlternatives)
      { 
        GetAlternatives(
          alternative,
          profileName,
          instanceName,
          maxPoints,
          result,
          forbiddenEdges,
          forbiddenZones);
      }
    }
    private Geo2DCoordDTO CheckForIntersections(
      List<Geo2DCoordDTO> route,
      Dictionary<string, GeoObjectDTO> forbiddenZones
    )
    {
      if (route == null || route.Count == 0)
      {
        return null;
      }


      foreach (var pt2Check in route)
      {
        foreach (var fZone in forbiddenZones.Values)
        {
          var polygon = fZone.location.coord as List<Geo2DCoordDTO>;

          if (polygon == null) { continue; }

          var b = IsPointInPolygon4(polygon, pt2Check);

          if (b)
          {
            return pt2Check;
          }
        }
      }
      return null;
    }

    private async Task<Dictionary<string, GeoObjectDTO>> GetForbiddenZones()
    {
      ObjPropsSearchDTO property_filter = new ObjPropsSearchDTO()
      {
        props = new List<KeyValueDTO>()
        {
          new KeyValueDTO()
          {
            prop_name = "layer_name",
            str_val = "forbidden_zones"
          }
        }
      };

      var props = await _mapService.GetPropByValuesAsync(
        property_filter,
        null,
        true,
        1000
      );

      var f_zones = props.Select(z => z.id).ToList();

      return  await _geoService.GetGeoObjectsAsync(f_zones);
    }

    private bool IsIntersectedForbidden(
      TreeEdgeDTO edge,
      Dictionary<string, GeoObjectDTO> forbiddenZones
    )
    {
      foreach (var fZone in forbiddenZones.Values)
      {
        var polygon = fZone.location.coord as List<Geo2DCoordDTO>;

        if (polygon == null) { continue; }

        bool bIsForbidden = IsPointInPolygon4(polygon, edge.Shape);

        if (bIsForbidden)
        {
          return true;
        }
      }

      return false;
    }

    [HttpPost]
    [Route("GetSmartRoute")]
    public async Task<ActionResult<List<List<Geo2DCoordDTO>>>> GetSmartRoute(RoutDTO routData)
    {
      string testProfile = "pedestrian";

      List<List<Geo2DCoordDTO>> bunchOfRouts = new List<List<Geo2DCoordDTO>>();

      var routRetPed = await _router.GetRoute(
        routData.InstanceName,
        testProfile,
        routData.Coordinates);

      var forbiddenZones = await GetForbiddenZones();

      var badPointPed = CheckForIntersections(routRetPed, forbiddenZones);

      if (badPointPed != null)
      {
        var startNode = new TreeEdgeDTO()
        {
          Shape = badPointPed
        };
        var routA = new HashSet<TreeEdgeDTO>();
        var forbiddenEdges = new HashSet<TreeEdgeDTO>();

        GetAlternatives(
          startNode,
          testProfile,
          routData.InstanceName,
          5,              
          routA,
          forbiddenEdges,
          forbiddenZones
        );

        _router.RemoveEdges(
          routData.InstanceName,
        testProfile, forbiddenEdges.Select(e => e.EdgeId).ToHashSet());

        foreach ( var r in routA)
        {
          List<Geo2DCoordDTO> aRout = new List<Geo2DCoordDTO>()
          {
            routData.Coordinates.First(),
            r.Shape,
            routData.Coordinates.Last()
          };

          var routRetPedA = await _router.GetRoute(
            routData.InstanceName,
            testProfile,
            aRout);
          bunchOfRouts.Add(routRetPedA);
        }       
      }


      if (routRetPed != null)
      {
        bunchOfRouts.Add(routRetPed);
      }

      return CreatedAtAction(nameof(GetSmartRoute), bunchOfRouts);
    }
  }
}