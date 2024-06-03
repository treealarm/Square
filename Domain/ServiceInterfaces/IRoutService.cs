using Domain.GeoDTO;
using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IRoutService
  {
    Task<long> InsertManyAsync(List<RoutLineDTO> newObjs);
    Task<List<RoutLineDTO>> GetByIdsAsync(List<string> ids);
    Task<List<string>> GetProcessedIdsAsync(List<string> ids);
    Task<List<RoutLineDTO>> GetRoutesByBox(BoxTrackDTO box);
    Task DeleteManyAsync(List<string> ids);
  }
}
