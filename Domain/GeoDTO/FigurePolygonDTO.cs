using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class FigurePolygonDTO : FigureZoomedDTO
  {
    //public double[][] geometry { get; set; }
    public GeometryPolygonDTO geometry { get; set; }
  }
}
