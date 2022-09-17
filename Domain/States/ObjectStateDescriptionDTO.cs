using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.States
{
  public class ObjectStateDescriptionDTO
  {
    public string id { get; set; }
    public bool alarm { get; set; }
    public string state { get; set; }
    public string state_descr { get; set; }
    public Color state_color { get; set; }
  }
}
