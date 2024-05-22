using Domain.GeoDTO;
using System;
using System.Collections.Generic;

namespace Domain.StateWebSock
{
  public class TrackPointDTO
  {
    public string id { get; set; } = default!;
    public GeoObjectDTO figure { get; set; } = default!;
    public DateTime timestamp { get; set; }
    public List<ObjExtraPropertyDTO> extra_props { get; set; } = default!;
  }
}
