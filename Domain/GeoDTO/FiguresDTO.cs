using Domain.GeoDTO;
using System.Collections.Generic;

namespace Domain
{
  public class FiguresDTO
  {
    public List<FigureCircleDTO> circles { get; set; } = new List<FigureCircleDTO>();
    public List<FigurePolygonDTO> polygons { get; set; } = new List<FigurePolygonDTO>();
    public List<FigurePolylineDTO> polylines { get; set; } = new List<FigurePolylineDTO>();
  }
}
