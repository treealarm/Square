using Domain;
using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  internal class MapUpdateService: IMapUpdateService
  {
    private readonly IMapService _mapService;
    private readonly IGeoService _geoService;
    private readonly ITrackService _tracksService;
    private readonly ILevelService _levelService;
    private readonly IDiagramService _diagramService;
    private readonly ITracksUpdateService _trackUpdateService;
    public MapUpdateService(
      IMapService mapService, 
      IGeoService geoService,
      ITrackService tracksService,
      ILevelService levelService,
      IDiagramService diagramService,
      ITracksUpdateService trackUpdateService)
    {
      _mapService = mapService;
      _geoService = geoService;
      _tracksService = tracksService;
      _levelService = levelService;
      _diagramService = diagramService;
      _trackUpdateService = trackUpdateService;
    }

    public async Task<FiguresDTO?> UpdateFigures(FiguresDTO statMarkers)
    {
      await _trackUpdateService.UpdateFigures(statMarkers);
      return statMarkers;
    }

    public async Task<ObjPropsDTO?> UpdateProperties(ObjPropsDTO updatedMarker)
    {
      if (string.IsNullOrEmpty(updatedMarker.id))
      {
        await _mapService.UpdateHierarchyAsync(new List<BaseMarkerDTO>() { updatedMarker });
      }

      var marker = await _mapService.GetAsync(updatedMarker?.id);

      if (marker is null)
      {
        return null;
      }

      marker.name = updatedMarker?.name;
      marker.parent_id = updatedMarker?.parent_id;

      await _mapService.UpdateHierarchyAsync(new List<BaseMarkerDTO>() { marker });



      await _mapService.UpdatePropAsync(updatedMarker);

      ObjExtraPropertyDTO? radius = null;
      ObjExtraPropertyDTO? zoom_level = null;
      ObjExtraPropertyDTO? geometry = null;

      if (updatedMarker?.extra_props != null)
      {
        radius = updatedMarker.extra_props.Where(p => p.prop_name == "radius").FirstOrDefault();
        zoom_level = updatedMarker.extra_props.Where(p => p.prop_name == "zoom_level").FirstOrDefault();
        geometry = updatedMarker.extra_props.Where(p => p.prop_name == "geometry").FirstOrDefault();
      }

      await _geoService.CreateOrUpdateGeoFromStringAsync(
        updatedMarker?.id,
        geometry?.str_val,
        radius?.str_val,
        zoom_level?.str_val
      );


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
      await _diagramService.RemoveAsync(listToDelete);
      return idsToDelete;
    }
  }
}
