using Domain;

namespace DataChangeLayer
{
  internal class TracksUpdateService: ITracksUpdateService
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
      return trackPointsInserted.Select (t => t.id!).ToList();
    }

    public async Task<List<string>> AddTracks(List<TrackPointDTO> movedMarkers)
    {      
      return await DoUpdateTracks(movedMarkers);
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
          GeoObjectDTO? circle;

          updatedFigs.TryGetValue(figure.id!, out circle);

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
