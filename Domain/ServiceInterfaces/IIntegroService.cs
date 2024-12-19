using Domain.Integro;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IIntegroService
  {
    public Task<Dictionary<string, IntegroDTO>> GetListByIdsAsync(List<string> ids);
  }
  internal interface IIntegroServiceInternal
  {
    public Task UpdateListAsync(List<IntegroDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
  }
}
