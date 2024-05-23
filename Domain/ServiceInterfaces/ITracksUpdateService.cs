using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ITracksUpdateService
  {
    public Task<FiguresDTO> UpdateFigures(FiguresDTO statMarkers);
    public Task<List<string>> AddTracks(List<TrackPointDTO> movedMarkers);
  }
}
