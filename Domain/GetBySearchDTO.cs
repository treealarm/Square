using System.Collections.Generic;

namespace Domain
{
  public class GetBySearchDTO
  {
    public List<BaseMarkerDTO>? list { get; set; }
    public string? search_id { get; set; }
  }
}
