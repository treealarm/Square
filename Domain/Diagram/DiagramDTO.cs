using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diagram
{
  public record DiagramDTO: FigureZoomedDTO
  {
    public DiagramCoordDTO geometry {  get; set; }
    public string region_id { get; set; }
    public string dgr_type { get; set; }
    public string background_img { get; set; }
    
  }
}
