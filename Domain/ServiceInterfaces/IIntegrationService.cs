using Domain.Diagram;
using Domain.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  }
}
