using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IRouter
  {
    Task<List<Geo2DCoordDTO>> GetRoute(string inst, List<Geo2DCoordDTO> coords);
  }
}
