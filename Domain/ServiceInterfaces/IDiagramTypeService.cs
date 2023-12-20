using Domain.Diagram;
using Domain.Rights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IDiagramTypeService
  {
    public Task UpdateListAsync(List<DiagramTypeDTO> obj2UpdateIn);
    public Task DeleteAsync(string id);
    public Task<Dictionary<string, DiagramTypeDTO>> GetListByTypeNamesAsync(List<string> typeName);
  }
}
