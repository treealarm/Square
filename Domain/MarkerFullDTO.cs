using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class MarkerFullDTO
  {
    public string id { get; set; }
    public string parent_id { get; set; }
    public string name { get; set; } = null;
    public GeoPartDTO geometry { get; set; }
  }
}
