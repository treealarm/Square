using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;
using PubSubLib;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RouterController : ControllerBase
  {
    private ITrackRouter _router;
    private IRoutService _routService;
    private readonly IMapService _mapService;
    private IPubService _pub;
    public RouterController(
      IPubService pubsub,
      IRoutService routService,
      ITrackRouter router,
      IMapService mapService
    )
    {
      _routService = routService;
      _router = router;
      _mapService = mapService;
      _pub = pubsub;
    }

    private async Task AddIdsByProperties(BoxTrackDTO box)
    {
      List<string> ids = null;

      if (box.property_filter != null && box.property_filter.props.Count > 0)
      {
        var props = await _mapService.GetPropByValuesAsync(
          box.property_filter,
          null,
          true,
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
        HttpClient client = new HttpClient();
        if (Startup.InDocker)
        {
          client.BaseAddress = new Uri(@"http://routermicroservice:7177");
        }
        else
        {
          client.BaseAddress = new Uri(@"http://localhost:7177");
        }
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response =
         await client.PostAsJsonAsync($"Router/GetSmartRoute", routData);

        response.EnsureSuccessStatusCode();

        // Deserialize the updated product from the response body.
        var s = await response.Content.ReadAsStringAsync();

        try
        {
          var routRets = JsonSerializer.Deserialize<List<List<Geo2DCoordDTO>>>(s);

          foreach (var r in routRets)
          {
            ret.Add(new RoutLineDTO()
            {
              figure = new GeoObjectDTO()
              {
                location = new GeometryPolylineDTO()
                {
                  coord = r
                }
              }
            });
          }
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
        await _routService.InsertManyAsync(newObjs);
      }
    }
  }
}
