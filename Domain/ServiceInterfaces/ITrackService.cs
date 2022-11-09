using Domain.GeoDTO;
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
    Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box);
    Task<TrackPointDTO> GetLastAsync(string figure_id, DateTime beforeTime);
    Task<TrackPointDTO> GetByIdAsync(string id);
    Task<List<TrackPointDTO>> GetFirstTracksByTime(
      DateTime? time_start,
      DateTime? time_end,
      List<string> ids
    );
  }
}
