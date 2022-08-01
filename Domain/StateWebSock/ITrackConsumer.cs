using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public interface ITrackConsumer
  {
    Task OnUpdatePosition(List<TrackPointDTO> movedMarkers);
  }
}
