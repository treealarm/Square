
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
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
