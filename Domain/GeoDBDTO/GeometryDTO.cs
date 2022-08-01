using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  public abstract class GeometryDTO<TCoordinates>: List<TCoordinates>
  {
  }
}
