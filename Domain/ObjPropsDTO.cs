using System;
using System.Collections.Generic;

namespace Domain
{
  public class ObjPropsDTO : FigureBaseDTO, IObjectProps
  {
    public List<ObjExtraPropertyDTO> extra_props { get; set; } = new List<ObjExtraPropertyDTO>();
  }
}
