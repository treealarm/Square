using Domain;

namespace DataChangeLayer
{
  internal class MapUpdateService: IMapUpdateService
  {
    private readonly IMapService _mapService;
    private readonly IGeoService _geoService;
    private readonly ILevelService _levelService;
    private readonly IDiagramUpdateService _diagramUpdateService;
    private readonly ITracksUpdateService _trackUpdateService;
    private readonly IIntegroUpdateService _integroUpdateService;
    public MapUpdateService(
      IMapService mapService, 
      IGeoService geoService,
      ITrackService tracksService,
      ILevelService levelService,
      IDiagramUpdateService diagramUpdateService,
      ITracksUpdateService trackUpdateService,
      IIntegroUpdateService integroUpdateService
    )
    {
      _mapService = mapService;
      _geoService = geoService;
      _levelService = levelService;
      _diagramUpdateService = diagramUpdateService;
      _trackUpdateService = trackUpdateService;
      _integroUpdateService = integroUpdateService;
    }

    public async Task<FiguresDTO?> UpdateFigures(FiguresDTO statMarkers)
    {
      await _trackUpdateService.UpdateFigures(statMarkers);
      return statMarkers;
    }

    public async Task<ObjPropsDTO?> UpdateProperties(ObjPropsDTO updatedMarker)
    {
      //if (string.IsNullOrEmpty(updatedMarker.id))
      {
        await _mapService.UpdateHierarchyAsync(new List<BaseMarkerDTO>() { updatedMarker });
      }

      var marker = await _mapService.GetAsync(updatedMarker?.id);

      if (marker is null)
      {
        return null;
      }

      await _mapService.UpdatePropAsync([updatedMarker!]);

      return updatedMarker;
    }
    public async Task<HashSet<string>> Delete(List<string> ids)
    {
      HashSet<string> idsToDelete = new HashSet<string>();

      foreach (var id in ids)
      {
        var marker = await _mapService.GetAsync(id);

        if (marker is null)
        {
          continue;
        }

        var markers = await _mapService.GetAllChildren(id);
        var bunchIds = markers.Select(m => m.id).ToHashSet();
        idsToDelete.Add(marker.id!);
        idsToDelete.UnionWith(bunchIds!);
      }

      var listToDelete = idsToDelete.ToList();

      await _geoService.RemoveAsync(listToDelete);
      await _mapService.RemoveAsync(listToDelete);
      await _diagramUpdateService.DeleteDiagrams(listToDelete);
      await _integroUpdateService.RemoveIntegros(listToDelete);
      return idsToDelete;
    }
  }
}
