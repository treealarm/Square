
using Domain.Values;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IValuesUpdateService
  {
    public Task UpdateValues(List<ValueDTO> obj2UpdateIn);
    public Task RemoveValues(List<string> ids);
  }
}
