using Domain.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  internal interface IDiagramServiceInternal
  {
    public Task UpdateListAsync(List<DiagramDTO> obj2UpdateIn);
  }
  public interface IDiagramService
  {
    public Task RemoveAsync(List<string> ids);
    public Task<Dictionary<string, DiagramDTO>> GetListByIdsAsync(List<string> ids);
  }
}
