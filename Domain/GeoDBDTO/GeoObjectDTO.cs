
namespace Domain
{
  public record GeoObjectDTO: BaseObjectDTO
  {
    public GeometryDTO? location { get; set; }
    public double? radius { get; set; }
    public string? zoom_level { get; set; }
  }
}
