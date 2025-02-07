

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IDiagramTypeUpdateService
  {
    public Task<List<string>> DeleteDiagramTypes(List<string> ids);
    public Task<GetDiagramTypesDTO> UpdateDiagramTypes(List<DiagramTypeDTO> dgr_types);
  }
}
