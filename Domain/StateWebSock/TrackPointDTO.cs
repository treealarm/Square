
using System;
using System.Collections.Generic;

namespace Domain
{
  public class TrackPointDTO
  {
    public string? id { get; set; }
    public GeoObjectDTO? figure { get; set; }
    public DateTime? timestamp { get; set; }
    public List<ObjExtraPropertyDTO>? extra_props { get; set; }
  }
}
