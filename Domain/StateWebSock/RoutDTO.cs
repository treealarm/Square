
using System.Collections.Generic;

namespace Domain
{
  public class RoutDTO
  {
    public string? InstanceName { get; set; }
    public string? Profile { get; set; }
    public List<Geo2DCoordDTO>? Coordinates { get; set; }
  }
}
