
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IGeoService
  {
    Task<Dictionary<string, GeoObjectDTO>> GetGeoObjectsAsync(List<string> ids);
    Task<Dictionary<string, GeoObjectDTO>> GetGeoAsync(BoxDTO box);
    Task<GeoObjectDTO> GetGeoObjectAsync(string id);
    Task<Dictionary<string, GeoObjectDTO>> CreateGeo(IEnumerable<FigureGeoDTO> figures);
    Task CreateOrUpdateGeoFromStringAsync(
      string? id,
      string? geometry,
      string? radius,
      string? zoom_level
    );
    Task<long> RemoveAsync(List<string> ids);
    Task<Dictionary<string, GeoObjectDTO>> GetGeoObjectNearestsAsync(
      List<string> ids,
      Geo2DCoordDTO ptDto,
      int limit
    );

    Task<Dictionary<string, GeoObjectDTO>> GetGeoIntersectAsync(
      GeometryDTO geoObject,
      HashSet<string> ids,
       bool bNot
      );
  }
}
