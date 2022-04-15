using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class FigurePolylineDTO : FigureBaseDTO
  {
    public double[][] geometry { get; set; }
  }
}
