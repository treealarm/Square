using Domain.GeoDBDTO;
namespace Domain.GeoDTO
{
  public class GeoObjectDTO
  {
    public string? id { get; set; }
    public GeometryDTO? location { get; set; }
    public double? radius { get; set; }
    public string? zoom_level { get; set; }
  }
}
