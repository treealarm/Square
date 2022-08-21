using DbLayer;
using Domain;
using Domain.GeoDBDTO;
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
  public class TracksController : ControllerBase
  {
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private ITrackConsumer _stateService;
    private readonly IGeoService _geoService;
    private BaseMarkerDTO _tracksRootFolder;
    private IRoutService _routService;
    private const string TRACKS_ROOT_NAME = "__tracks";
    private IRouter _router;
    public TracksController(
      IMapService mapsService,
      ITrackService tracksService,
      IRoutService routService,
      IGeoService geoService,
      ITrackConsumer stateService,
      IRouter router
    )
    {
      _mapService = mapsService;
      _tracksService = tracksService;
      _routService = routService;
      _stateService = stateService;
      _geoService = geoService;
      _router = router;
    }

    private async Task<BaseMarkerDTO> GetTracksRoot()
    {
      if (_tracksRootFolder == null)
      {
        var list_of_roots = await _mapService.GetByNameAsync(TRACKS_ROOT_NAME);

        if (list_of_roots.Count == 0)
        {
          _tracksRootFolder = new BaseMarkerDTO()
          {
            name = TRACKS_ROOT_NAME
          };

          await _mapService.CreateOrUpdateHierarchyObject(_tracksRootFolder);
        }
        else
        {
          _tracksRootFolder = list_of_roots.First();
        }

        return _tracksRootFolder;
      }

      return _tracksRootFolder;
    }

    private async Task EnsureTracksRoot(BaseMarkerDTO marker)
    {
      if (string.IsNullOrEmpty(marker.parent_id))
      {
        var root = await GetTracksRoot();
        marker.parent_id = root.id;
      }
    }

    [HttpPost]
    [Route("Empty")]
    public ActionResult<string> Empty([FromBody] string s)
    {
      return CreatedAtAction(nameof(Empty), s);
    }

    private async Task<TrackPointDTO> GetLast(TrackPointDTO newPoint)
    {
      return await _tracksService.GetLastAsync(newPoint.figure.id);
    }
    private async Task DoUpdateTracks(FiguresDTO movedMarkers)
    {
      var trackPoints = new List<TrackPointDTO>();      

      foreach (var figure in movedMarkers.circles)
      {
        await EnsureTracksRoot(figure);
        await _mapService.CreateOrUpdateHierarchyObject(figure);

        var newPoint = new TrackPointDTO()
        {
          figure = await _geoService.CreateGeoPoint(figure)
        };

        trackPoints.Add(newPoint);        
      }

      foreach (var figure in movedMarkers.polygons)
      {
        await EnsureTracksRoot(figure);
        await _mapService.CreateOrUpdateHierarchyObject(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      foreach (var figure in movedMarkers.polylines)
      {
        await EnsureTracksRoot(figure);
        await _mapService.CreateOrUpdateHierarchyObject(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      await _tracksService.InsertManyAsync(trackPoints);

      // Add Routs.

      var routs = new List<TrackPointDTO>();

      foreach (var trackPoint in trackPoints)
      {
        if (trackPoint.figure.location is not GeometryCircleDTO)
        {
          continue;
        }

        var newPoint = trackPoint;
        var lastPoint = await GetLast(newPoint);

        if (lastPoint != null)
        {
          var coords = new List<Geo2DCoordDTO>();
          var p1 = (newPoint.figure.location as GeometryCircleDTO).coord;
          coords.Add(p1);
          var p2 = (lastPoint.figure.location as GeometryCircleDTO).coord;
          coords.Add(p2);
          var routRet = await _router.GetRoute(string.Empty, coords);

          if (routRet != null && routRet.Count > 0)
          {
            routRet.Insert(0, p1);
            routRet.Add(p2);
            var polyLine = new GeometryPolylineDTO();
            newPoint.figure.location = polyLine;
            polyLine.coord = routRet;
            routs.Add(newPoint);
          }
        }
      }

      if (routs.Count > 0)
      {
        await _routService.InsertManyAsync(routs);
      }      

      await _stateService.OnUpdateTrackPosition(trackPoints);
    }

    [HttpPost]
    [Route("AddTracks")]
    public async Task<ActionResult<FiguresDTO>> AddTracks(FiguresDTO movedMarkers)
    {
      await DoUpdateTracks(movedMarkers);
      return CreatedAtAction(nameof(AddTracks), movedMarkers);
    }

    [HttpPost]
    [Route("UpdateTracks")]
    public async Task<ActionResult<bool>> UpdateTracks(FiguresDTO movedMarkers)
    {
      await DoUpdateTracks(movedMarkers);

      return CreatedAtAction(nameof(UpdateTracks), true);
    }

    [HttpGet]
    [Route("GetTracks")]
    public async Task<ActionResult<List<TrackPointDTO>>> GetTracks()
    {
      var trackPoints = await _tracksService.GetAsync();

      return CreatedAtAction(nameof(GetTracks), trackPoints);
    }

    [HttpPost]
    [Route("GetRoute")]
    public async Task<ActionResult<List<Geo2DCoordDTO>>> GetRoute(RoutDTO routData)
    {
      var routRet = await _router.GetRoute(routData.InstanceName, routData.Coordinates);

      return CreatedAtAction(nameof(GetTracks), routRet);
    }
  }
}
