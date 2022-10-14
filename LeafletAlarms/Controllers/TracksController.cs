using DbLayer;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


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

    public TracksController(
      IMapService mapsService,
      ITrackService tracksService,
      IRoutService routService,
      IGeoService geoService,
      ITrackConsumer stateService
    )
    {
      _mapService = mapsService;
      _tracksService = tracksService;
      _routService = routService;
      _stateService = stateService;
      _geoService = geoService;
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

          await _mapService.UpdateHierarchyAsync(new List<BaseMarkerDTO>() { _tracksRootFolder });
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

    [HttpGet()]
    [Route("GetHello")]
    public  List<string> GetHello()
    {
      return new List<string>() { "Hello world" };
    }

    static Dictionary<string, TrackPointDTO> _cashLast = new Dictionary<string, TrackPointDTO>();
    static private object _locker = new object();

    private async Task<TrackPointDTO> GetLast(TrackPointDTO newPoint)
    {
      TrackPointDTO retVal;

      lock(_locker)
      {        
        _cashLast.TryGetValue(newPoint.figure.id, out retVal);
        var s = JsonSerializer.Serialize<TrackPointDTO>(newPoint);
        var fig = JsonSerializer.Deserialize<TrackPointDTO>(s);
        _cashLast[newPoint.figure.id] = fig;
      }
      
      if (retVal == null)
      {
        retVal = await _tracksService.GetLastAsync(newPoint.figure.id, newPoint.id);
      }
      return retVal;
    }
    private async Task<Dictionary<string, TimeSpan>> DoUpdateTracks(FiguresDTO movedMarkers)
    {
      var trackPoints = new List<TrackPointDTO>();
      Dictionary<string, TimeSpan> timing = new Dictionary<string, TimeSpan>();
      
      DateTime t1 = DateTime.Now;

      foreach (var figure in movedMarkers.circles)
      {
        await EnsureTracksRoot(figure);
      }

      foreach (var figure in movedMarkers.polygons)
      {
        await EnsureTracksRoot(figure);
      }

      foreach (var figure in movedMarkers.polylines)
      {
        await EnsureTracksRoot(figure);
      }

      await _mapService.UpdateHierarchyAsync(movedMarkers.circles);

      foreach (var figure in movedMarkers.circles)
      {       
        await _mapService.UpdatePropNotDeleteAsync(figure);

        var newPoint =
          new TrackPointDTO() {
            figure = await _geoService.CreateGeoPoint(figure)
          };

        if (figure.extra_props != null)
        {
          var propTimeStamp = figure.extra_props
          .Where(p => p.prop_name == "timestamp")
          .FirstOrDefault();

          if (propTimeStamp != null)
          {
            newPoint.timestamp = DateTime
              .Parse(
                propTimeStamp.str_val
              ).ToUniversalTime();
            figure.extra_props.Remove(propTimeStamp);
          }
        }        

        trackPoints.Add(newPoint);
      }

      DateTime t2 = DateTime.Now;
      timing["circlesInsert"] = t2 - t1;

      await _mapService.UpdateHierarchyAsync(movedMarkers.polygons);

      foreach (var figure in movedMarkers.polygons)
      {         
        await _mapService.UpdatePropNotDeleteAsync(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      await _mapService.UpdateHierarchyAsync(movedMarkers.polylines);

      foreach (var figure in movedMarkers.polylines)
      {
        await EnsureTracksRoot(figure);
        
        await _mapService.UpdatePropNotDeleteAsync(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      t1 = DateTime.Now;
      var trackPointsInserted = await _tracksService.InsertManyAsync(trackPoints);
      t2 = DateTime.Now;

      timing["tracksInsert"] = t2 - t1;

      // Add Routs.
      t1 = DateTime.Now;
      var routs = new List<RoutLineDTO>();

      foreach (var trackPoint in trackPointsInserted)
      {
        if (trackPoint.figure.location is not GeometryCircleDTO)
        {
          continue;
        }

        var newPoint = trackPoint;        
        var lastPoint = await GetLast(newPoint);

        if (lastPoint != null)
        {
          var newRout = new RoutLineDTO();
          

          newRout.figure = new GeoObjectDTO();
          newRout.figure.id = newPoint.figure.id;
          newRout.figure.zoom_level = newPoint.figure.zoom_level;

          var lineCoord = new List<Geo2DCoordDTO>();
          var p1 = (newPoint.figure.location as GeometryCircleDTO).coord;
          lineCoord.Add(p1);

          var p2 = (lastPoint.figure.location as GeometryCircleDTO).coord;
          lineCoord.Add(p2);

          newRout.id_start = lastPoint.id;
          newRout.id_end = newPoint.id;
          newRout.ts_start = lastPoint.timestamp;
          newRout.ts_end = newPoint.timestamp;

          var polyLine = new GeometryPolylineDTO();
          newRout.figure.location = polyLine;
          polyLine.coord = lineCoord;

          routs.Add(newRout);
        }
      }
      t2 = DateTime.Now;
      timing["routsBuild"] = t2 - t1;

      if (routs.Count > 0)
      {
        t1 = DateTime.Now;
        await _routService.InsertManyAsync(routs);
        t2 = DateTime.Now;
        timing["routsInsert"] = t2 - t1;
      }
      t1 = DateTime.Now;
      await _stateService.OnUpdateTrackPosition(trackPoints);
      timing["UpdateTracksCall"] = t2 - t1;
      t2 = DateTime.Now;

      return timing;
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
    public async Task<ActionResult<Dictionary<string, TimeSpan>>> UpdateTracks(FiguresDTO movedMarkers)
    {
      DateTime t1 = DateTime.Now;      
      var dic = await DoUpdateTracks(movedMarkers);
      DateTime t2 = DateTime.Now;

      dic["All"] =  t2 - t1;
      return CreatedAtAction(nameof(UpdateTracks), dic);
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
    [Route("GetTracksByBox")]
    public async Task<ActionResult<List<TrackPointDTO>>> GetTracksByBox(BoxTrackDTO box)
    {
      await AddIdsByProperties(box);

      var trackPoints = await _tracksService.GetTracksByBox(box);

      return CreatedAtAction(nameof(GetTracksByBox), trackPoints);
    }
  }
}
