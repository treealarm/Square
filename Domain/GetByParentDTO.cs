using System.Collections.Generic;

namespace Domain
{
  public class GetByParentDTO
  {
    public List<MarkerDTO>? children { get; set; }
    public List<BaseMarkerDTO>? parents { get; set; }
    public string? parent_id { get; set; }
    public string? start_id { get; set; }
    public string? end_id { get; set; }
  }
}
