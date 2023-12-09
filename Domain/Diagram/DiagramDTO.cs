using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public class DiagramDTO
  {
    public string cur_diagram_id { get; set; }
    public List<BaseMarkerDTO> parents { get; set; }
  }
}
