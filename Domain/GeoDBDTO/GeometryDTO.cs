using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  public class GeometryDTO
  {
    public string GetFigureType()
    {
      if (this is GeometryCircleDTO)
      {
        return "Point";
      }

      if (this is GeometryPolygonDTO)
      {
        return "Polygon";
      }

      if (this is GeometryPolylineDTO)
      {
        return "LineString";
      }

      return string.Empty;
    }

    public virtual string GetJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}
