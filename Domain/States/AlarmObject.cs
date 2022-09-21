using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.States
{
  public class AlarmObject: BaseMarkerDTO
  {
    public bool alarm { get; set; }
    public int children_alarms { get; set; }
  }
}
