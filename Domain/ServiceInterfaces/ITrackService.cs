using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ITrackService
  {
    Task<List<TrackPointDTO>> InsertManyAsync(List<TrackPointDTO> newObjs);
    Task<List<TrackPointDTO>> GetAsync();
    Task<TrackPointDTO> GetLastAsync(string figure_id, string ignoreTrackId);
  }
}
