using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Domain;
using Domain.PubSubTopics;

namespace LeafletAlarms.Services
{
  public class TracksUpdateService
  {
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private readonly IGeoService _geoService;

    private IPubService _pub;

    public TracksUpdateService(
      IMapService mapsService,
      ITrackService tracksService,
      IGeoService geoService,
      IPubService pub
    )
    {
      _mapService = mapsService;
      _tracksService = tracksService;
      _geoService = geoService;
      _pub = pub;
    }


    private async Task<List<string>> DoUpdateTracks(List<TrackPointDTO> trackPoints)
    {
      var trackPointsInserted = await _tracksService.InsertManyAsync(trackPoints);

      await _pub.Publish(Topics.OnUpdateTrackPosition, trackPoints);

      return trackPointsInserted.Select (t => t.id).ToList();
    }

    public async Task<List<string>> AddTracks(List<TrackPointDTO> movedMarkers)
    {      
      return await DoUpdateTracks(movedMarkers);
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
