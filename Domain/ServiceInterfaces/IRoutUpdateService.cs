
using Domain.StateWebSock;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IRoutUpdateService
  {
    public Task<long> InsertRoutes(List<RoutLineDTO> newObjs);
  }
}
