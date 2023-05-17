using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ITrackRouter
  {
    Task<List<Geo2DCoordDTO>> GetRoute(string inst, string profile, List<Geo2DCoordDTO> coords);
    public bool IsMapExist(string inst);
  }
}
