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

    [HttpGet()]
    [Route("GetHello")]
    public  List<string> GetHello()
    {
      return new List<string>() { "Hello world" };
    }

    private async Task<TrackPointDTO> GetLast(TrackPointDTO newPoint)
    {
      return await _tracksService.GetLastAsync(newPoint.figure.id, newPoint.id);
    }
    private async Task DoUpdateTracks(FiguresDTO movedMarkers)
    {
      var trackPoints = new List<TrackPointDTO>();      

      foreach (var figure in movedMarkers.circles)
      {
        var propTimeStamp = figure.extra_props
          .Where(p => p.prop_name == "timestamp")
          .FirstOrDefault();

        var newPoint = new TrackPointDTO()
        {
          figure = await _geoService.CreateGeoPoint(figure)
        };

        if (propTimeStamp != null)
        {
          newPoint.timestamp = DateTime
            .Parse(
              propTimeStamp.str_val
            ).ToUniversalTime();
          figure.extra_props.Remove(propTimeStamp);
        }

        await EnsureTracksRoot(figure);
        await _mapService.CreateOrUpdateHierarchyObject(figure);
        await _mapService.UpdatePropNotDeleteAsync(figure);

        
        
        trackPoints.Add(newPoint);        
      }

      foreach (var figure in movedMarkers.polygons)
      {
        await EnsureTracksRoot(figure);
        await _mapService.CreateOrUpdateHierarchyObject(figure);
        await _mapService.UpdatePropNotDeleteAsync(figure);

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
        await _mapService.UpdatePropNotDeleteAsync(figure);

        trackPoints.Add(
          new TrackPointDTO()
          {
            figure = await _geoService.CreateGeoPoint(figure)
          }
        );
      }

      var trackPointsInserted = await _tracksService.InsertManyAsync(trackPoints);

      // Add Routs.
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
