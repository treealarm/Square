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
    public Task<List<GeoObjectDTO>> GetGeoObjectsAsync(List<string> ids);
    public Task<List<GeoObjectDTO>> GetGeoAsync(BoxDTO box);
    public Task<GeoObjectDTO> GetGeoObjectAsync(string id);
    public Task<GeoObjectDTO> CreateGeoPoint(FigureBaseDTO figure);
    public Task CreateOrUpdateGeoFromStringAsync(
      string id,
      string geometry,
      string type,
      string radius,
      string zoom_level
    );
    public Task<long> RemoveAsync(List<string> ids);
  }
}
