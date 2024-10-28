using Domain.Diagram;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  internal interface IDiagramServiceInternal
  {
    public Task UpdateListAsync(List<DiagramDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
  }
  public interface IDiagramService
  {    
    public Task<Dictionary<string, DiagramDTO>> GetListByIdsAsync(List<string> ids);
  }
}
