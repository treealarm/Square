﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public record MarkerDTO: BaseMarkerDTO
  {
    public bool has_children { get; set; }
  }
}
