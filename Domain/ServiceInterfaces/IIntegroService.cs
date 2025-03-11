
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IIntegroService
  {
    public Task<Dictionary<string, IntegroDTO>> GetListByIdsAsync(List<string> ids);
    public Task<Dictionary<string, IntegroDTO>> GetListByType(string i_name, string i_type);
  }
  internal interface IIntegroServiceInternal
  {
    public Task UpdateListAsync(List<IntegroDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);    
  }
}
