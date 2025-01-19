using Common;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using LeafletAlarmsGrpc;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using ValhallaLib;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RouterController : ControllerBase
  {
    private readonly ValhallaRouter _valhalla_router;
    private readonly IRoutService _routService;
    private readonly IRoutUpdateService _routUpdateService;
    private readonly IMapService _mapService;
    private readonly IPubService _pub;
    public RouterController(
      IPubService pubsub,
      IRoutService routService,
      IMapService mapService,
      ValhallaRouter vrouter,
      IRoutUpdateService routUpdateService
    )
    {
      _routService = routService;
      _mapService = mapService;
      _pub = pubsub;
      _valhalla_router = vrouter;
      _routUpdateService = routUpdateService;
    }

    private async Task AddIdsByProperties(BoxTrackDTO box)
    {
      List<string> ids = null;

      if (box.property_filter != null && box.property_filter.props.Count > 0)
      {
        var props = await _mapService.GetPropByValuesAsync(
          box.property_filter,
          null,
          1,
          1000
        );
        ids = props.Select(i => i.id).ToList();

        if (box.ids == null)
        {
          box.ids = new List<string>();
        }
        box.ids.AddRange(ids);
      }
    }

    [HttpPost]
    [Route("GetRoute")]
    public async Task<ActionResult<List<List<Geo2DCoordDTO>>>> GetRoute(RoutDTO routData)
    {
      //var routRet = await _router.GetRoute(routData.InstanceName, routData.Profile, routData.Coordinates);
      var ret = new List<RoutLineDTO>();

      try
      {

        var start = routData.Coordinates.FirstOrDefault();
        var end = routData.Coordinates.LastOrDefault();

        var routRets = await _valhalla_router.GetRoute(
          new ProtoCoord() { Lat = start.Lat, Lon = start.Lon },
          new ProtoCoord() { Lat = end.Lat, Lon = end.Lon }
        );

        try
        {

          var rcoord = new List<Geo2DCoordDTO>();

          foreach (var r in routRets)
          {
            rcoord.Add(new Geo2DCoordDTO()
            {
              Lat = r.Lat,
              Lon = r.Lon
            });
          }
          ret.Add(new RoutLineDTO()
          {
            figure = new GeoObjectDTO()
            {
              location = new GeometryPolylineDTO()
              {
                coord = rcoord
              }
            }
          });
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"{ex.Message}");
      }

      return CreatedAtAction(nameof(GetRoute), ret);
    }

    [HttpPost]
    [Route("GetRoutesByTracksIds")]
    public async Task<List<RoutLineDTO>> GetRoutesByTracksIds(List<string> ids)
    {
      var geo = await _routService.GetByIdsAsync(ids);

      await _pub.Publish(Topics.OnRequestRoutes, ids);

      return geo;
    }

    [HttpPost]
    [Route("GetRoutesByBox")]
    public async Task<List<RoutLineDTO>> GetRoutesByBox(BoxTrackDTO box)
    {
      if (
        box.time_start == null &&
        box.time_end == null
      )
      {
        // We do not search without time diapason.
        return new List<RoutLineDTO>();
      }

      await AddIdsByProperties(box);
      var geo = await _routService.GetRoutesByBox(box);
      return geo;
    }

    [HttpPost]
    [Route("InsertRoutes")]
    public async Task InsertRoutes(List<RoutLineDTO> newObjs)
    {
      if (newObjs.Count > 0)
      {
        await _routUpdateService.InsertRoutes(newObjs);
      }
    }
  }
}
