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
    public DiagramCoordDTO geometry { get; set; } = default!;
    public string region_id { get; set; } = default!;
    public string dgr_type { get; set; } = default!;
    public string background_img { get; set; } = default!;

  }
}
