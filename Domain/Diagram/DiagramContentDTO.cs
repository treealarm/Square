using System.Collections.Generic;
using Domain.DiagramType;

namespace Domain.Diagram
{
  public record DiagramContentDTO
  {
    public string diagram_id { get; set; }
    public List<DiagramDTO>? content { get; set; }
    public List<ObjPropsDTO>? content_props { get; set; }
    public List<DiagramTypeDTO>? dgr_types { get; set; }
    public List<BaseMarkerDTO>? parents { get; set; }
    public int depth {  get; set; }
  }
}
