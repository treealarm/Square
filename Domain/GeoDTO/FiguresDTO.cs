using Domain.GeoDTO;
using System.Collections.Generic;

namespace Domain
{
  public class FiguresDTO
  {
    public List<FigureGeoDTO> figs { get; set; } = new List<FigureGeoDTO>();

    public bool IsEmpty()
    {
      if (figs?.Count > 0)
      {
        return false;
      }
      return true;
    }
  }
}
