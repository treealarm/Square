using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class FigureCircleDTO : FigureBaseDTO
  {
    public GeoCircleDTO geometry { get; set; }
  }
}
