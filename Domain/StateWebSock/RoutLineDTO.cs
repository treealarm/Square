﻿using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class RoutLineDTO
  {
    public string id { get; set; }
    public GeoObjectDTO figure { get; set; }
    public string id_start { get; set; }
    public string id_end { get; set; }
  }
}