﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class LevelDTO
  {
    public string id { get; set; }

    public string zoom_level { get; set; }
    public int zoom_min { get; set; }
    public int zoom_max { get; set; }
  }
}
