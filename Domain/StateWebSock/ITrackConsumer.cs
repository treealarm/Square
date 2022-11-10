using Domain.GeoDTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public interface ITrackConsumer
  {
    void OnUpdateTrackPosition(List<TrackPointDTO> movedMarkers);
    Task PushAsync(HttpContext context, WebSocket webSocket);
  }
}
