using Domain.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  internal interface IIntegrationServiceInternal
  {
    public Task UpdateListAsync(List<IntegrationDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
  }
  public interface IIntegrationService
  {    
    public Task<Dictionary<string, IntegrationDTO>> GetByParentIdsAsync(
      List<string> parent_ids,
      string start_id,
      string end_id,
      int count
    );
    public Task<Dictionary<string, bool>> GetHasChildrenAsync(
      List<string> parent_ids
    );
  }
}
