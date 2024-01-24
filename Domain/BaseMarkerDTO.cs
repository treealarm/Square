using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public record BaseMarkerDTO
  {
    public string id { get; set; }
    public string parent_id { get; set; }
    public string name { get; set; }
    public string external_type { get; set; }
  }
}
