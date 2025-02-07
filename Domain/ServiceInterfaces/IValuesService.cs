using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IValuesService
  {
    public Task<Dictionary<string, ValueDTO>> GetListByIdsAsync(List<string> ids);
    public Task<Dictionary<string, ValueDTO>> GetListByOwnersAsync(List<string> owners);
  }

  internal interface IValuesServiceInternal
  {
    public Task UpdateListAsync(List<ValueDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
    public Task<Dictionary<string, ValueDTO>> UpdateValuesFilteredByNameAsync(List<ValueDTO> obj2UpdateIn);
  }
}
