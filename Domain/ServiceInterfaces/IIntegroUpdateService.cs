using Domain.Integro;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IIntegroUpdateService
  {
    public Task UpdateIntegros(List<IntegroDTO> obj2UpdateIn);
    public Task RemoveIntegros(List<string> ids);
  }
}
