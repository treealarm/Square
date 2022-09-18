using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  public class StateWebSocketHandler: ITrackConsumer, IStateConsumer
  {
    private IStateService _stateService;
    private IGeoService _geoService;
    private ILevelService _levelService;
    public StateWebSocketHandler(
      IStateService stateService,
      IGeoService geoService,
      ILevelService levelService
    )
    {
      _stateService = stateService;
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
        _geoService,
        _levelService,
        _stateService
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

    public async Task OnStateChanged(List<ObjectStateDTO> state)
    {
      foreach (var sock in StateSockets)
      {
        await sock.Value.OnStateChanged(state);
      }
    }
  }
}
