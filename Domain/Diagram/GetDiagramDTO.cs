using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public class GetDiagramDTO
  {
    public DiagramDTO container_diagram { get; set; }
    public List<DiagramDTO> content { get; set; }
  }
}
