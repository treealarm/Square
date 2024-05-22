using Domain;
using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  public class DataChangeService: IDataChangeService
  {
    private readonly IMapService _mapService;
    private readonly IGeoService _geoService;
    private readonly ITrackService _tracksService;
    private readonly ILevelService _levelService;
    private readonly IDiagramService _diagramService;
    public DataChangeService(
      IMapService mapService, 
      IGeoService geoService,
      ITrackService tracksService,
      ILevelService levelService,
      IDiagramService diagramService)
    {
      _mapService = mapService;
      _geoService = geoService;
      _tracksService = tracksService;
      _levelService = levelService;
      _diagramService = diagramService;
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
  }
}
