using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class FigurePolylineDTO : FigureZoomedDTO
  {
    public GeometryDTO geometry { get; set; }
  }
}
