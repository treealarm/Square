using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public class DiagramDTO: FigureZoomedDTO
  {
    public DiagramCoordDTO geometry {  get; set; }
    public string dgr_type { get; set; }
  }
}
