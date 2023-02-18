using Domain.Rights;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IRightService
  {
    public Task UpdateListAsync(List<ObjectRightsDTO> obj2UpdateIn);
    public Task DeleteAsync(string id);
    public Task<Dictionary<string, ObjectRightsDTO>> GetListByIdsAsync(List<string> ids);
  }
}
