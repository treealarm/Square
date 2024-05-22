using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  public class GeometryPolygonDTO : GeometryDTO
  {
    public new List<Geo2DCoordDTO>? coord
    {
      get
      {
        if (_coord == null)
        {
          _coord = new List<Geo2DCoordDTO>();
        }
        return _coord as List<Geo2DCoordDTO>;
      }
      set
      {
        _coord = value;
      }
    }

    public override string GetJson()
    {
      return JsonSerializer.Serialize(this);
    }

    public override Geo2DCoordDTO? GetCentroid()
    {
      if (coord == null || coord.Count == 0)
      {
        return null;
      }
      var retVal = new Geo2DCoordDTO() { Lon = 0, Lat = 0 };

      foreach (var c in coord)
      {
        retVal.Lon += c.Lon;
        retVal.Lat += c.Lat;
      }

      retVal.Lon /= coord.Count;
      retVal.Lat /= coord.Count;
      return retVal;
    }
  }
}
