
using Domain.Diagram;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IDiagramUpdateService
  {
    Task<List<DiagramDTO>> UpdateDiagrams(List<DiagramDTO> dgrs);
  }
}
