using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  public class GeometryPolygonDTO : GeometryDTO
  {
    private List<Geo2DCoordDTO> _coord;
    public List<Geo2DCoordDTO> coord
    {
      get
      {
        if (_coord == null)
        {
          _coord = new List<Geo2DCoordDTO>();
        }
        return _coord;
      }
      set
      {
        _coord = value;
      }
    }
  }
}
