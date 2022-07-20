using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class FigureZoomedDTO: FigureBaseDTO
  {
    public int min_zoom { get; set; }
    public int max_zoom { get; set; }    
  }
}
