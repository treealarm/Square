
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IRoutUpdateService
  {
    public Task<long> InsertRoutes(List<RoutLineDTO> newObjs);
  }
}
