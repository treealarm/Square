
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IValuesUpdateService
  {
    public Task UpdateValues(List<ValueDTO> obj2UpdateIn);
    public Task RemoveValues(List<string> ids);
    public Task<Dictionary<string, ValueDTO>> UpdateValuesFilteredByNameAsync(List<ValueDTO> obj2UpdateIn);
  }
}
