
using System.Collections.Generic;

using System.Threading.Tasks;

namespace Domain
{
    public interface IDiagramTypeService
  {
    public Task UpdateListAsync(List<DiagramTypeDTO> obj2UpdateIn);
    public Task DeleteAsync(List<string> ids);
    public Task<Dictionary<string, DiagramTypeDTO>> GetListByTypeNamesAsync(List<string> typeName);
    public Task<Dictionary<string, DiagramTypeDTO>> GetListByTypeIdsAsync(List<string> ids);
    public Task<Dictionary<string, DiagramTypeDTO>> GetDiagramTypesByFilter(GetDiagramTypesByFilterDTO filter);
  }
}
