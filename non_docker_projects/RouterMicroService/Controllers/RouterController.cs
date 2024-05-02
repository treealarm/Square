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

    private bool CheckCoordForIntersection(
      Geo2DCoordDTO pt2Check,
      Dictionary<string, GeoObjectDTO> forbiddenZones
    )
    {
      foreach (var fZone in forbiddenZones.Values)
      {
        var polygon = fZone.location.coord as List<Geo2DCoordDTO>;

        if (polygon == null) { continue; }

        var b = IsPointInPolygon4(polygon, pt2Check);

        if (b)
        {
          return true;
        }
      }
      return false;
    }
    private List<Geo2DCoordDTO> CheckForIntersections(
      List<Geo2DCoordDTO> route,
      Dictionary<string, GeoObjectDTO> forbiddenZones
    )
    {
      if (route == null || route.Count == 0)
      {
        return null;
      }
      var ret = new List<Geo2DCoordDTO>();

      foreach (var pt2Check in route)
      {
        if (CheckCoordForIntersection(pt2Check, forbiddenZones))
        {
          ret.Add(pt2Check);
        }
      }

      if (ret.Count == 0)
      {
        return null;
      }
      return ret;
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

      return await _geoService.GetGeoObjectsAsync(f_zones);
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

    private async Task<List<Geo2DCoordDTO>> TryToBuildRoute(
      Dictionary<string, GeoObjectDTO> forbidden,
      string instanceName,
      string testProfile,      
      List<Geo2DCoordDTO> routScooter
      )
    {
      var routRetScooter = await _router.GetRoute(
        instanceName,
        testProfile,
        routScooter);

      var intersect = CheckForIntersections(
          routRetScooter,
          forbidden
        );

      if ((intersect != null))
      {
        _router.SetLowWeight(
          instanceName,
          testProfile,
          intersect,
          0);
        return null;
      }

      return routRetScooter;
    }

    [HttpPost]
    [Route("GetSmartRoute")]
    public async Task<ActionResult<List<List<Geo2DCoordDTO>>>> GetSmartRoute(RoutDTO routData)
    {
      var forbidden = await GetForbiddenZones();

      string pedestrianProfile = "pedestrian";

      var parking0 = await GetNearestParkings(routData.Coordinates.First(), 1);
      var parking1 = await GetNearestParkings(routData.Coordinates.Last(), 1);

      var p0 = parking0.Values.First().location.coord as Geo2DCoordDTO;
      var routPedestrian = new List<Geo2DCoordDTO>
      {
        p0,
        routData.Coordinates.First()
      };

      var routRetPed0 = await _router.GetRoute(
        routData.InstanceName,
        pedestrianProfile,
        routPedestrian);

      var p1 = parking1.Values.Last().location.coord as Geo2DCoordDTO;
      routPedestrian = new List<Geo2DCoordDTO>
      {
        p1,
        routData.Coordinates.Last()
      };

      var routRetPed1 = await _router.GetRoute(
        routData.InstanceName,
        pedestrianProfile,
        routPedestrian);


      List<List<Geo2DCoordDTO>> bunchOfRouts = new List<List<Geo2DCoordDTO>>();

      var routScooter = new List<Geo2DCoordDTO>
      {
        p0,
        p1
      };

      var routRetPed = new List<Geo2DCoordDTO>();

      for (int i = 0; i <10;i++)
      {
        routRetPed = await TryToBuildRoute(
          forbidden,
          routData.InstanceName,
          pedestrianProfile,
          routScooter);

        if (routRetPed != null)
        {
          break;
        }
      }
      var routRetScooter =  new List<Geo2DCoordDTO>();

      for (int i = 0; i < 10; i++)
      {
        routRetScooter = await TryToBuildRoute(
          forbidden,
          routData.InstanceName,
          "bicycle",
          routScooter);

        if (routRetScooter != null)
        {
          break;
        }
      }

      if (routRetPed0 != null)
      {
        bunchOfRouts.Add(routRetPed0);
      }

      if (routRetScooter != null)
      {
        bunchOfRouts.Add(routRetScooter);

        var intersect = CheckForIntersections(
          routRetScooter,
          forbidden
        );
      }

      if (routRetPed != null)
      {
        var intersect = CheckForIntersections(
          routRetPed,
          forbidden
        );
        bunchOfRouts.Add(routRetPed);
      }

      if (routRetPed1 != null)
      {
        bunchOfRouts.Add(routRetPed1);
      }

      return CreatedAtAction(nameof(GetSmartRoute), bunchOfRouts);
    }
  }
}