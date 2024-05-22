using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class RoutLineDTO
  {
    public string id { get; set; } = default!;
    public GeoObjectDTO figure { get; set; } = default!;
    public string id_start { get; set; } = default!;
    public string id_end { get; set; } = default!;
    public DateTime? ts_start { get; set; }
    public DateTime? ts_end { get; set; }
  }
}
