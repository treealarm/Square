using Domain.Diagram;
using Domain.Values;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IValuesService
  {
    public Task<Dictionary<string, ValueDTO>> GetListByIdsAsync(List<string> ids);
    public Task<Dictionary<string, ValueDTO>> GetListByOwnerAsync(string owner);
  }

  internal interface IValuesServiceInternal
  {
    public Task UpdateListAsync(List<ValueDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
  }
}
