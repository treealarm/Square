using System.Collections.Generic;
using Domain.DiagramType;

namespace Domain.Diagram
{
    public record GetDiagramDTO
  {
    public List<DiagramDTO> content { get; set; } = default!;
    public List<DiagramTypeDTO> dgr_types { get; set; } = default!;
    public DiagramDTO parent { get; set; } = default!;
    public List<BaseMarkerDTO> parents { get; set; } = default!;
    public int depth {  get; set; }
  }
}
