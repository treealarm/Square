using DbLayer;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
    public RouterController(
      IRoutService routService,
      ITrackRouter router,
      IMapService mapService
    )
    {
      _routService = routService;
      _router = router;
      _mapService = mapService;
    }

    private async Task AddIdsByProperties(BoxTrackDTO box)
    {
      List<string> ids = null;

      if (box.property_filter != null && box.property_filter.props.Count > 0)
      {
        var props = await _mapService.GetPropByValuesAsync(
          box.property_filter,
          null,
          null,
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
    public async Task<ActionResult<List<Geo2DCoordDTO>>> GetRoute(RoutDTO routData)
    {
      var routRet = await _router.GetRoute(routData.InstanceName, routData.Coordinates);

      return CreatedAtAction(nameof(GetRoute), routRet);
    }


    [HttpPost]
    [Route("GetRoutsByBox")]
    public async Task<List<RoutLineDTO>> GetRoutsByBox(BoxTrackDTO box)
    {
      await AddIdsByProperties(box);
      var geo = await _routService.GetRoutsByBox(box);
      return geo;
    }

    [HttpPost]
    [Route("InsertRouts")]
    public async Task InsertRouts(List<RoutLineDTO> newObjs)
    {
      if (newObjs.Count > 0)
      {
        await _routService.InsertManyAsync(newObjs);
      }
    }
  }
}
