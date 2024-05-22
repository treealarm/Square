using System.Collections.Generic;

namespace Domain
{
  public class GetByParentDTO
  {
    public List<MarkerDTO> children { get; set; } = default!;
    public List<BaseMarkerDTO> parents { get; set; } = default!;
    public string parent_id { get; set; } = default!;
    public string start_id { get; set; } = default!;
    public string end_id { get; set; } = default!;
  }
}
