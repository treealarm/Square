﻿using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class TrackPointDTO
  {
    public string id { get; set; }
    public GeoObjectDTO figure { get; set; }
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
  }
}