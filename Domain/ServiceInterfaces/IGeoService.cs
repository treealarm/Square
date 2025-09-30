
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

    Task<long> RemoveAsync(List<string> ids);
  }
}
