using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Logic
{
  public class TrackZonePosition
  {
    public TrackPointDTO Track { get; set; }
    public bool IsInZone { get; set; }
  }
}
