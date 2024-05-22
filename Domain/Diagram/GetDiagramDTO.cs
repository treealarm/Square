using System.Collections.Generic;
using Domain.DiagramType;

namespace Domain.Diagram
{
    public record GetDiagramDTO
  {
    public List<DiagramDTO>? content { get; set; }
    public List<DiagramTypeDTO>? dgr_types { get; set; }
    public DiagramDTO? parent { get; set; }
    public List<BaseMarkerDTO>? parents { get; set; }
    public int depth {  get; set; }
  }
}
