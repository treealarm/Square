using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using PubSubLib;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

namespace LeafletAlarms.Services
{
  public class WebSockListService : IWebSockList
  {
    private IStateService _stateService;
    private IGeoService _geoService;
    private ILevelService _levelService;
    private IMapService _mapService;
    private ISubService _sub;
    private IPubService _pub;

    public WebSockListService(
      IStateService stateService,
      IGeoService geoService,
      ILevelService levelService,
      IMapService mapService,
      ISubService sub,
      IPubService pub
    )
    {
      _stateService = stateService;
      _geoService = geoService;
      _levelService = levelService;
      _mapService = mapService;
      _sub = sub;
      _pub = pub;

      _sub.Subscribe(Topics.NewRoutBuilt, NewRoutBuilt);
      _sub.Subscribe(Topics.OnStateChanged, OnStateChanged);
      _sub.Subscribe(Topics.OnBlinkStateChanged, OnBlinkStateChanged);
      _sub.Subscribe(Topics.OnUpdateTrackPosition, OnUpdateTrackPosition);
    }

    ~WebSockListService()
    {
      _sub.Unsubscribe(Topics.NewRoutBuilt, NewRoutBuilt);
      _sub.Unsubscribe(Topics.OnStateChanged, OnStateChanged);
      _sub.Unsubscribe(Topics.OnBlinkStateChanged, OnBlinkStateChanged);
      _sub.Unsubscribe(Topics.OnUpdateTrackPosition, OnUpdateTrackPosition);
    }
    public static ConcurrentDictionary<string, StateWebSocket> StateSockets { get; set; } =
      new ConcurrentDictionary<string, StateWebSocket>();
    public async Task PushAsync(object context, WebSocket webSocket)
    {
      var con = context as HttpContext;

      StateWebSocket stateWs = new StateWebSocket(
       con,
        webSocket,
        _geoService,
        _levelService,
        _stateService,
        _mapService,
        _pub
      );

      StateSockets.TryAdd(con.Connection.Id, stateWs);

      try
      {
        await stateWs.ProcessAcceptedSocket();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      
      try
      {
        if (!webSocket.CloseStatus.HasValue)
        {
          await webSocket
            .CloseAsync(
            WebSocketCloseStatus.InternalServerError,
            "",
            CancellationToken.None
          );
        }        
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      StateSockets.TryRemove(con.Connection.Id, out var sock);
    }

    public async Task OnUpdateTrackPosition(string channel, string message)
    {
      var movedMarkers = JsonSerializer.Deserialize<List<TrackPointDTO>>(message);

      if (movedMarkers == null)
      {
        return;
      }

      foreach (var sock in StateSockets)
      {
        sock.Value.OnUpdateTrackPosition(movedMarkers);
      }
      await Task.CompletedTask;
    }

    public void OnUpdateTracks(List<string> movedMarkers)
    {
      foreach (var sock in StateSockets)
      {
        sock.Value.OnUpdateTracks(movedMarkers);
      }
    }    

    public async Task OnStateChanged(string channel, string message)
    {
      var state = JsonSerializer.Deserialize<List<ObjectStateDTO>>(message);

      if (state == null)
      {
        return;
      }

      foreach (var sock in StateSockets)
      {
        await sock.Value.OnStateChanged(state);
      }
    }

    public async Task OnBlinkStateChanged(string channel, string message)
    {
      var state = JsonSerializer.Deserialize<List<AlarmState>>(message);

      if (state == null)
      {
        return;
      }

      foreach (var sock in StateSockets)
      {
        await sock.Value.OnBlinkStateChanged(state);
      }
    }

    async Task NewRoutBuilt(string channel, string message)
    {
      var routEnds = JsonSerializer.Deserialize<List<string>>(message);

      if (routEnds == null)
      {
        return;
      }

      foreach (var sock in StateSockets)
      {
        sock.Value.OnUpdateTracks(routEnds);
      }
      await Task.CompletedTask;
    }
  }
}
