using System.Collections.Generic;

namespace Domain.StateWebSock
{
  public interface ITrackConsumer
  {
    void OnUpdateTrackPosition(List<TrackPointDTO> movedMarkers);    
  }
}
