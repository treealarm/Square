﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public class DiagramTypeDTO
  {
    public string id { get; set; }
    public string name { get; set; }
    public string src { get; set; }
    public List<DiagramTypeRegionDTO> regions { get; set; }
  }
}