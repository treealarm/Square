
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IRightService
  {
    public Task<long> UpdateListAsync(List<ObjectRightsDTO> obj2UpdateIn);
    public Task<long> DeleteAsync(string id);
    public Task<Dictionary<string, ObjectRightsDTO>> GetListByIdsAsync(List<string> ids);
  }
}
