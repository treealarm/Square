using System.Collections.Generic;
using Domain;

namespace Domain
{
  public record DiagramContentDTO
  {
    public required string diagram_id { get; set; }
    public List<DiagramDTO>? content { get; set; }
    public List<DiagramTypeDTO>? dgr_types { get; set; }
    public List<BaseMarkerDTO>? parents { get; set; }
    public List<BaseMarkerDTO>? children { get; set; }
    public int depth {  get; set; }
  }
}
