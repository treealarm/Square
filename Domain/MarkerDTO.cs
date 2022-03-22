using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class MarkerDTO: TreeMarkerDTO
  {
    public double[] points { get; set; }
    public bool has_children { get; set; }
  }
}
