using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IGeoService
  {
    Task<List<GeoObjectDTO>> GetGeoObjectsAsync(List<string> ids);
    Task<List<GeoObjectDTO>> GetGeoAsync(BoxDTO box);
    Task<GeoObjectDTO> GetGeoObjectAsync(string id);
    Task<Dictionary<string, GeoObjectDTO>> CreateGeo(IEnumerable<FigureGeoDTO> figures);
    Task CreateOrUpdateGeoFromStringAsync(
      string id,
      string geometry,
      string radius,
      string zoom_level
    );
    Task<long> RemoveAsync(List<string> ids);
  }
}
