using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface ITracksUpdateService
  {
    public Task<FiguresDTO> UpdateFigures(FiguresDTO statMarkers);
    public Task<List<string>> AddTracks(List<TrackPointDTO> movedMarkers);
  }
}
