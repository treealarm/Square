using Domain.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  internal interface IIntegrationLeafsServiceInternal
  {
    Task UpdateListAsync(List<IntegrationLeafDTO> obj2UpdateIn);
    Task RemoveAsync(List<string> ids);
  }
  public interface IIntegrationLeafsService
  {    
    Task<Dictionary<string, IntegrationLeafDTO>> GetByParentIdsAsync(
      List<string> parent_ids
    );
  }
}
