
using Domain.GeoDBDTO;
using System.Collections.Generic;

namespace Domain
{
  public class BoxDTO
  {
    public double[] wn { get; set; } = default!;
    public double[] es { get; set; } = default!;
    public double? zoom { get; set; }
    public List<string> ids { get; set; } = default!;
    public int? count { get; set; }
    public ObjPropsSearchDTO property_filter { get; set; } = default!;
    public List<GeometryDTO> zone { get; set; } = default!;
    public bool not_in_zone { get; set; } = false;
  }
}
