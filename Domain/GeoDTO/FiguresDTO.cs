using Domain.GeoDTO;
using System.Collections.Generic;

namespace Domain
{
  public class FiguresDTO
  {
    public List<FigureCircleDTO> circles { get; set; } = new List<FigureCircleDTO>();
    public List<FigurePolygonDTO> polygons { get; set; } = new List<FigurePolygonDTO>();
    public List<FigurePolylineDTO> polylines { get; set; } = new List<FigurePolylineDTO>();

    public bool IsEmpty()
    {
      if (circles?.Count > 0)
      {
        return false;
      }
      if (polygons?.Count > 0)
      {
        return false;
      }
      if (polylines?.Count > 0)
      {
        return false;
      }
      return true;
    }
  }
}
