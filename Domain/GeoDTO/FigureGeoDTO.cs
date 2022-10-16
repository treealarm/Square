using Domain.GeoDBDTO;
using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class FigureGeoDTO : FigureZoomedDTO
  {
    public GeometryDTO geometry { get; set; }
    public double? radius { get; set; }    
  }
}
