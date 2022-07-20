using DbLayer;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  public class StateWebSocketHandler
  {
    private MapService _mapService;
    public StateWebSocketHandler(MapService mapsService)
    {
      _mapService = mapsService;
    }
    public static ConcurrentDictionary<string, StateWebSocket> StateSockets { get; set; } =
      new ConcurrentDictionary<string, StateWebSocket>();
    public async Task PushAsync(HttpContext context, WebSocket webSocket)
    {
      StateWebSocket stateWs = new StateWebSocket(context, webSocket, _mapService);
      StateSockets.TryAdd(context.Connection.Id, stateWs);
      await stateWs.ProcessAcceptedSocket();
      StateSockets.TryRemove(context.Connection.Id, out var sock);
    }

    public async Task OnUpdatePosition(List<TrackPoint> movedMarkers)
    {
      foreach (var sock in StateSockets)
      {
        await sock.Value.OnUpdatePosition(movedMarkers);
      }
    }
  }
}
