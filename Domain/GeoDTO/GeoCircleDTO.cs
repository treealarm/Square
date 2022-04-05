using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class GeoCircleDTO : GeoPartDTO
  {
    public double lng { get; set; }
    public double lat { get; set; }
  }
}
