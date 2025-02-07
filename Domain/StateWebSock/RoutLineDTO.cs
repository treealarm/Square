
using System;

namespace Domain
{
  public class RoutLineDTO
  {
    public string? id { get; set; }
    public GeoObjectDTO? figure { get; set; }
    public string? id_start { get; set; }
    public string? id_end { get; set; }
    public DateTime? ts_start { get; set; }
    public DateTime? ts_end { get; set; }
  }
}
