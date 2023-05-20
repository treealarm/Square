using Dapr.Client;
using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Itinero;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text.Json;

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
      var routRet = await _router.GetRoute(routData.InstanceName, routData.Profile, routData.Coordinates);
      return CreatedAtAction(nameof(GetRoute), routRet);
    }

    public static bool IsPointInPolygon4(List<Geo2DCoordDTO> polygon, Geo2DCoordDTO testPoint)
    {
      bool result = false;
      int j = polygon.Count() - 1;
      for (int i = 0; i < polygon.Count(); i++)
      {
        if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
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

    private async Task<List<Geo2DCoordDTO>> CheckForIntersections(List<Geo2DCoordDTO> route)
    {
      if (route == null || route.Count == 0)
      {
        return null;
      }

      var geoObject = new GeometryPolylineDTO()
      {
        coord = route
      };

      var forbiddenZones = new List<GeoObjectDTO>();
      var intersected = await _geoService.GetGeoIntersectAsync(geoObject);

      if (intersected != null)
      {
        var propObjects = await _mapService.GetPropsAsync(intersected.Keys.ToList());
        List<Geo2DCoordDTO> routeNew = new List<Geo2DCoordDTO>();
        
        foreach (var prop in propObjects.Values)
        {
          var bForbidden = prop.extra_props
            .Where(p => p.prop_name == "layer_name" && p.str_val == "forbidden_zones")
            .Any();

          if (bForbidden)
          {
            if (intersected.TryGetValue(prop.id, out var forbiddenZone))
            {                
              forbiddenZones.Add(forbiddenZone);
            }
          }
        }

        foreach (var pt2Check in route)
        {
          var bInPoly = false;

          foreach (var fZone in forbiddenZones)
          {
            var polygon = fZone.location.coord as List<Geo2DCoordDTO>;

            if (polygon == null) { continue; }

            bInPoly = IsPointInPolygon4(polygon, pt2Check);

            if (bInPoly)
            {
              break;
            }
          }

          if (!bInPoly)
          {
            routeNew.Add(pt2Check);
          }
        }
        return routeNew;
      }
      return route;
    }

    [HttpPost]
    [Route("GetSmartRoute")]
    public async Task<ActionResult<List<List<Geo2DCoordDTO>>>> GetSmartRoute(RoutDTO routData)
    {
      List<List<Geo2DCoordDTO>> bunchOfRouts = new List<List<Geo2DCoordDTO>> ();

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

      var ped0 = routData.Coordinates.First();
      var ped1 = routData.Coordinates.Last();

      var ids = props.Select(i => i.id).ToList();

      var geoParks0 = await _geoService.GetGeoObjectNearestsAsync(
        ids,
        ped0,
        2
      );

      var geoParks1 = await _geoService.GetGeoObjectNearestsAsync(
        ids,
        ped1,
        2
      );

      foreach( var park0 in geoParks0.Values )
      {
        foreach (var park1 in geoParks1.Values)
        {
          var p0 = park0.location as GeometryCircleDTO;
          var p1 = park1.location as GeometryCircleDTO;

          if (p0 == null ||  p1 == null)
          { continue; }

          List<Geo2DCoordDTO> scooterPark = new List<Geo2DCoordDTO>()
          {
            p0.coord,
            p1.coord
          };

          List<Geo2DCoordDTO> pedestrian0 = new List<Geo2DCoordDTO>()
          {
            ped0,
            p0.coord
          };

          List<Geo2DCoordDTO> pedestrian1 = new List<Geo2DCoordDTO>()
          {
            p1.coord,
            ped1
          };

          var routPed0 = await _router.GetRoute(routData.InstanceName, "pedestrian", pedestrian0);
          var routPed1 = await _router.GetRoute(routData.InstanceName, "pedestrian", pedestrian1);
          var routRetPed = await _router.GetRoute(routData.InstanceName, 
            "pedestrian",
            scooterPark);

          var routRetBi = await _router.GetRoute(routData.InstanceName,
            "bicycle",
            scooterPark);

          routRetPed = await CheckForIntersections(routRetPed);

          if (routRetPed != null)
          {
            routRetPed.InsertRange(0, routPed0);
            routRetPed.AddRange(routPed1);
            bunchOfRouts.Add(routRetPed);
          }

          if (routRetBi != null)
          {
            routRetBi.InsertRange(0, routPed0);
            routRetBi.AddRange(routPed1);
            bunchOfRouts.Add(routRetBi);
          }
        }
      }      

      return CreatedAtAction(nameof(GetSmartRoute), bunchOfRouts);
    }
  }
}