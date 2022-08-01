using DbLayer;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  public class StateWebSocketHandler: ITrackConsumer
  {
    private MapService _mapService;
    private GeoService _geoService;
    private LevelService _levelService;
    public StateWebSocketHandler(
      MapService mapsService,
      GeoService geoService,
      LevelService levelService
    )
    {
      _mapService = mapsService;
      _geoService = geoService;
      _levelService = levelService;
    }
    public static ConcurrentDictionary<string, StateWebSocket> StateSockets { get; set; } =
      new ConcurrentDictionary<string, StateWebSocket>();
    public async Task PushAsync(HttpContext context, WebSocket webSocket)
    {
      StateWebSocket stateWs = new StateWebSocket(
        context,
        webSocket,
        _mapService,
        _geoService,
        _levelService
      );
      StateSockets.TryAdd(context.Connection.Id, stateWs);
      await stateWs.ProcessAcceptedSocket();
      StateSockets.TryRemove(context.Connection.Id, out var sock);
    }

    public async Task OnUpdateTrackPosition(List<TrackPointDTO> movedMarkers)
    {
      foreach (var sock in StateSockets)
      {
        await sock.Value.OnUpdateTrackPosition(movedMarkers);
      }
    }
  }
}
