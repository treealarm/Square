
using Domain.GeoDBDTO;
using System.Collections.Generic;

namespace Domain
{
  public class BoxDTO
  {
    public double[] wn { get; set; }
    public double[] es { get; set; }
    public double? zoom { get; set; }
    public List<string> ids { get; set; }
    public int? count { get; set; }
    public ObjPropsSearchDTO? property_filter { get; set; }
    public List<GeometryDTO> zone { get; set; }
    public bool not_in_zone { get; set; } = false;
  }
}
