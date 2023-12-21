using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public class GetDiagramDTO
  {
    public List<DiagramDTO> content { get; set; }
    public DiagramDTO parent { get; set; }
    public List<DiagramTypeDTO> dgr_types { get; set; }
  }
}
