using DbLayer;
using Domain;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
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
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private ITrackConsumer _stateService;
    private readonly IGeoService _geoService;

    public TracksController(
      IMapService mapsService,
      ITrackService tracksService,
      IGeoService geoService,
      ITrackConsumer stateService
    )
    {
      _mapService = mapsService;
      _tracksService = tracksService;
      _stateService = stateService;
      _geoService = geoService;
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
      var trackPoints = new List<TrackPointDTO>();

      foreach (var figure in movedMarkers.circles)
      {
        await _mapService.CreateCompleteObject(figure);
        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      foreach (var figure in movedMarkers.polygons)
      {
        await _mapService.CreateCompleteObject(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      foreach (var figure in movedMarkers.polylines)
      {
        await _mapService.CreateCompleteObject(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      await _tracksService.InsertManyAsync(trackPoints);

      await _stateService.OnUpdateTrackPosition(trackPoints);

      return CreatedAtAction(nameof(AddTracks), movedMarkers);
    }
  }
}
