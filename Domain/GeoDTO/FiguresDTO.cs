using Domain.GeoDTO;
using System.Collections.Generic;

namespace Domain
{
  public class FiguresDTO
  {
    public List<FigureGeoDTO> circles { get; set; } = new List<FigureGeoDTO>();
    public List<FigureGeoDTO> polygons { get; set; } = new List<FigureGeoDTO>();
    public List<FigureGeoDTO> polylines { get; set; } = new List<FigureGeoDTO>();

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
