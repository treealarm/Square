using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class RoutDTO
  {
    public string InstanceName { get; set; } = default!;
    public string Profile { get; set; } = default!;
    public List<Geo2DCoordDTO> Coordinates { get; set; } = default!;
  }
}
