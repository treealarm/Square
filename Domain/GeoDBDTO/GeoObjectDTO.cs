using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class GeoObjectDTO
  {
    public string id { get; set; } = default!;
    public GeometryDTO location { get; set; } = default!;
    public double? radius { get; set; }
    public string zoom_level { get; set; } = default!;
  }
}
