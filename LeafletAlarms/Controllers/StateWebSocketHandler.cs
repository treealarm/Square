using DbLayer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  public class StateWebSocketHandler
  {
    private MapService _mapsService;
    public StateWebSocketHandler(MapService mapsService)
    {
      _mapsService = mapsService;
    }
    public static ConcurrentDictionary<string, StateWebSocket> StateSockets { get; set; } =
      new ConcurrentDictionary<string, StateWebSocket>();
    public async Task PushAsync(HttpContext context, WebSocket webSocket)
    {
      StateWebSocket stateWs = new StateWebSocket(context, webSocket);
      StateSockets.TryAdd(context.Connection.Id, stateWs);
      await stateWs.ProcessAcceptedSocket();
      StateSockets.TryRemove(context.Connection.Id, out var sock);
    }
  }
}
