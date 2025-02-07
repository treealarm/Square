
namespace Domain
{
  public record FigureGeoDTO : FigureZoomedDTO
  {
    public GeometryDTO? geometry { get; set; }
    public double? radius { get; set; }

    public double? GetRadius()
    {
      if (radius == null)
      {
        return 0;
      }
      return radius;
    }
  }
}
