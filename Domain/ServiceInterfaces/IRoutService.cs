using Domain.GeoDTO;
using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IRoutService
  {
    Task InsertManyAsync(List<RoutLineDTO> newObjs);
    Task<List<RoutLineDTO>> GetAsync();
    Task<List<RoutLineDTO>> GetRoutsByBox(BoxTrackDTO box);
  }
}
