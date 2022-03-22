using DbLayer;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms
{
  public class DTOConverter
  {
    public static MarkerDTO GetMarkerDTO(Marker marker)
    {
      return new MarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id,
        points = marker.points
      };
    }

    public static TreeMarkerDTO GetreeMarkerDTO(Marker marker)
    {
      return new TreeMarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }
  }
}
