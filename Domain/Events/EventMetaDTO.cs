﻿using System.Collections.Generic;

namespace Domain
{
  public class EventMetaDTO
  {
    public List<ObjExtraPropertyDTO>? extra_props { get; set; }
    public List<ObjExtraPropertyDTO>? not_indexed_props { get; set; }
  }
}
