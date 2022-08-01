using DbLayer;
using Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TracksController : ControllerBase
  {
    private readonly MapService _mapService;
    private StateWebSocketHandler _stateService;

    public TracksController(MapService mapsService, StateWebSocketHandler stateService)
    {
      _mapService = mapsService;
      _stateService = stateService;
    }

    [HttpPost]
    [Route("Empty")]
    public ActionResult<string> Empty([FromBody] string s)
    {
      return CreatedAtAction(nameof(Empty), s);
    }

    [HttpPost]
    [Route("AddTracks")]
    public async Task<IActionResult> AddTracks(FiguresDTO movedMarkers)
    {
      List<DBTrackPoint> trackPoints = new List<DBTrackPoint>();

      foreach (var figure in movedMarkers.circles)
      {
        trackPoints.Add(
          new DBTrackPoint()
          {
            figure = await _mapService.CreateCompleteObject(figure)
          }
        );
      }

      foreach (var figure in movedMarkers.polygons)
      {
        trackPoints.Add(
          new DBTrackPoint()
          {
            figure = await _mapService.CreateCompleteObject(figure)
          }
        );
      }

      foreach (var figure in movedMarkers.polylines)
      {
        trackPoints.Add(
          new DBTrackPoint()
          {
            figure = await _mapService.CreateCompleteObject(figure)
          }
        );
      }

      await _mapService.TracksServ.InsertManyAsync(trackPoints);

      //await _stateService.OnUpdatePosition(trackPoints);

      return CreatedAtAction(nameof(AddTracks), movedMarkers);
    }
  }
}
