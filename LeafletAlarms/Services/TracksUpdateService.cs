using Domain.GeoDTO;
using Domain.NonDto;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Domain;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Rewrite;
using PubSubLib;

namespace LeafletAlarms.Services
{
  public class TracksUpdateService
  {
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private readonly IGeoService _geoService;
    private BaseMarkerDTO _tracksRootFolder;
    private const string TRACKS_ROOT_NAME = "__tracks";

    private IPubSubService _pubsub;

    public TracksUpdateService(
      IMapService mapsService,
      ITrackService tracksService,
      IGeoService geoService,
      IPubSubService pubsub
    )
    {
      _mapService = mapsService;
      _tracksService = tracksService;
      _geoService = geoService;
      _pubsub = pubsub;
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
          _tracksRootFolder = list_of_roots.First().Value;
        }

        return _tracksRootFolder;
      }

      return _tracksRootFolder;
    }

    private async Task EnsureTracksRoot(BaseMarkerDTO marker)
    {
      // Set parents for moving objects, not for "tracks only".

      if (string.IsNullOrEmpty(marker.parent_id) && 
        !string.IsNullOrEmpty(marker.id)
      )
      {
        var root = await GetTracksRoot();
        marker.parent_id = root.id;
      }
    }

    private async Task<List<string>> DoUpdateTracks(List<TrackPointDTO> trackPoints)
    {
      var trackPointsInserted = await _tracksService.InsertManyAsync(trackPoints);

      _pubsub.PublishNoWait(Topics.OnUpdateTrackPosition, JsonSerializer.Serialize(trackPoints));

      return trackPointsInserted.Select (t => t.id).ToList();
    }

    public async Task<List<string>> AddTracks(List<TrackPointDTO> movedMarkers)
    {      
      return await DoUpdateTracks(movedMarkers);
    }

    private async Task AddIdsByProperties(BoxDTO box)
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

    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
    {
      if (
        box.time_start == null &&
        box.time_end == null
      )
      {
        // We do not search without time diapason.
        return new List<TrackPointDTO>();
      }

      //await AddIdsByProperties(box);

      var trackPoints = await _tracksService.GetTracksByBox(box);

      if (box.sort < 0)
      {
        trackPoints = trackPoints.OrderByDescending(f => f.timestamp).ToList();
      }

      return trackPoints;
    }

    public async Task<TrackPointDTO> GetTrackById(string id)
    {
      var trackPoint = await _tracksService.GetByIdAsync(id);

      return trackPoint;
    }

    public async Task<FiguresDTO> UpdateFigures(FiguresDTO statMarkers)
    {
      await _mapService.UpdateHierarchyAsync(statMarkers.figs);
      await _mapService.UpdatePropNotDeleteAsync(statMarkers.figs);
      var updatedFigs = await _geoService.CreateGeo(statMarkers.figs);

      if (statMarkers.add_tracks)
      {
        var trackPoints = new List<TrackPointDTO>();

        foreach (var figure in statMarkers.figs)
        {
          GeoObjectDTO circle;

          updatedFigs.TryGetValue(figure.id, out circle);

          var newPoint =
            new TrackPointDTO()
            {
              figure = circle,
              timestamp = DateTime.UtcNow,
              extra_props = figure.extra_props
            };

          trackPoints.Add(newPoint);
        }

        await AddTracks(trackPoints);
      }

      return statMarkers;
    }
  }
}
