using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public record GetDiagramDTO
  {
    public List<DiagramDTO> content { get; set; }
    public List<DiagramTypeDTO> dgr_types { get; set; }
    public string parent_id { get; set; }
    public List<BaseMarkerDTO> parents { get; set; }
    public int depth {  get; set; }
  }
}
