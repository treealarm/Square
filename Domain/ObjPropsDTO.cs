using System;
using System.Collections.Generic;
using Domain.ObjectInterfaces;

namespace Domain
{
    public record ObjPropsDTO : BaseMarkerDTO, IObjectProps, IObjectZoom
  {
    public List<ObjExtraPropertyDTO>? extra_props { get; set; } = new List<ObjExtraPropertyDTO>();
    public int? zoom_min { get; set; }
    public int? zoom_max { get; set; }
  }
}
