using DbLayer;
using Domain;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LeafletAlarms.Controllers
{  
  public class StateWebSocket
  {
    private HttpContext _context;
    private WebSocket _webSocket;
    private MapService _mapService;
    System.Timers.Timer tmr;
    HashSet<string> _dicIds = new HashSet<string>();
    object _locker = new object();
    void InitTimer()
    {
      tmr = new System.Timers.Timer();
      tmr.Interval = 1000;
      tmr.AutoReset = false;
      tmr.Elapsed += new ElapsedEventHandler(OnElapsed);
      tmr.Enabled = true;
    }

    private async void OnElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      // let timer start ticking
      tmr.Enabled = true;

      List<MarkerVisualState> stateList = new List<MarkerVisualState>();

      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "set_visual_states",
        data = stateList
      };      

      lock (_locker)
      {
        if (_dicIds.Count == 0)
        {
          return;
        }

        int i = 1;
        Random rnd = new Random();

        foreach (var id in _dicIds)
        {
          i++;
          

          MarkerVisualState state = new MarkerVisualState()
          {
            id = id,
            color = System.Drawing.ColorTranslator.ToHtml(
              System.Drawing.Color.FromArgb(255, rnd.Next(1, 254), rnd.Next(1, 254), rnd.Next(1, 254)))
          };

          stateList.Add(state);
        }
      }

      await SendPacket(packet);            
    }

    public StateWebSocket(
      HttpContext context,
      WebSocket webSocket,
      MapService mapsService
    )
    {
      _mapService = mapsService;
      _context = context;
      _webSocket = webSocket;

      InitTimer();
    }

    public async Task ProcessAcceptedSocket()
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult result = 
        await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

      while (!result.CloseStatus.HasValue)
      {
        string s = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

        StateBaseDTO json = JsonSerializer.Deserialize<StateBaseDTO>(s);

        if (json.action.ToString() == "set_box")
        {
          BoxDTO setBox = JsonSerializer.Deserialize<BoxDTO>(json.data.ToString()); ;

          if (setBox != null)
          {
            await OnSetBox(setBox);
          }
        }
        var replay = JsonSerializer.SerializeToUtf8Bytes(json);

        await _webSocket.SendAsync(
          new ArraySegment<byte>(buffer, 0, replay.Length),
          result.MessageType,
          result.EndOfMessage,
          CancellationToken.None
        );

        result = await _webSocket.ReceiveAsync(
          new ArraySegment<byte>(buffer),
          CancellationToken.None
        );
      }

      tmr.Enabled = false;

      await _webSocket.CloseAsync(
        result.CloseStatus.Value,
        result.CloseStatusDescription,
        CancellationToken.None
      );
    }

    public async Task OnUpdatePosition(List<string> movedMarkers)
    {
      List<string> toUpdate;

      lock (_locker)
      {
        toUpdate = movedMarkers.Where(m => _dicIds.Contains(m)).ToList();
      }

      if (toUpdate.Count > 0)
      {
        StateBaseDTO packet = new StateBaseDTO()
        {
          action = "set_ids2update",
          data = toUpdate
        };

        await SendPacket(packet);
      }
    }

    private async Task SendPacket(StateBaseDTO packet)
    {
      var buffer = JsonSerializer.SerializeToUtf8Bytes(packet);

      await _webSocket.SendAsync(
        new ArraySegment<byte>(buffer, 0, buffer.Length),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );
    }

    private async Task OnSetBox(BoxDTO box)
    {
      var geo = await _mapService.GeoServ.GetGeoAsync(box);

      lock(_locker)
      {
        _dicIds.Clear();

        foreach (var item in geo)
        {
          _dicIds.Add(item.id);
        }
      }
    }
  }
}
