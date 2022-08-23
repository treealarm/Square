using Domain.GeoDBDTO;
using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class FigureCircleDTO : FigureZoomedDTO
  {
    public GeometryCircleDTO geometry { get; set; }
    public double? radius { get; set; }
  }
}
