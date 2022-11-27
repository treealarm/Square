using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class ConsumerService : ITrackConsumer, IStateConsumer, IWebSockList
  {
    private IStateService _stateService;
    private IGeoService _geoService;
    private ILevelService _levelService;
    private IMapService _mapService;
    private IIdsQueue _stateIdsQueueService;
    private PubSubService _pubsub;

    public ConsumerService(
      IStateService stateService,
      IGeoService geoService,
      ILevelService levelService,
      IMapService mapService,
      IIdsQueue stateIdsQueueService,
      PubSubService pubsub
    )
    {
      _stateService = stateService;
      _geoService = geoService;
      _levelService = levelService;
      _mapService = mapService;
      _stateIdsQueueService = stateIdsQueueService;
      _pubsub = pubsub;
      _pubsub.Subscribe("LogicTriggered", LogicTriggered);
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
        _stateService,
        _mapService,
        _stateIdsQueueService
      );

      StateSockets.TryAdd(context.Connection.Id, stateWs);

      try
      {
        await stateWs.ProcessAcceptedSocket();
      }
      catch(Exception ex)
      {

      }
      
      try
      {
        if (!webSocket.CloseStatus.HasValue)
        {
          await webSocket
            .CloseAsync(WebSocketCloseStatus.InternalServerError, "", CancellationToken.None);
        }        
      }
      catch(Exception)
      { }

      StateSockets.TryRemove(context.Connection.Id, out var sock);
    }

    public void OnUpdateTrackPosition(List<TrackPointDTO> movedMarkers)
    {
      foreach (var sock in StateSockets)
      {
        sock.Value.OnUpdateTrackPosition(movedMarkers);
      }
    }

    public async Task OnStateChanged(List<ObjectStateDTO> state)
    {
      foreach (var sock in StateSockets)
      {
        await sock.Value.OnStateChanged(state);
      }
    }

    public async Task OnBlinkStateChanged(List<AlarmObject> state)
    {
      foreach (var sock in StateSockets)
      {
        await sock.Value.OnBlinkStateChanged(state);
      }
    }

    void LogicTriggered(string channel, object message)
    {
      foreach (var sock in StateSockets)
      {
        sock.Value.LogicTriggered(message);
      }
    }
  }
}
