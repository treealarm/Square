
using System.Collections.Generic;

namespace Domain.GeoDTO
{
  public class FigureZoomedDTO: FigureBaseDTO, IObjectProps
  {
    public string zoom_level { get; set; }

    // Optional property to update properties.
    public List<ObjExtraPropertyDTO> extra_props { get; set; }
  }
}
