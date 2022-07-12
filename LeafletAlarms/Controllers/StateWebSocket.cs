using Domain;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{  
  public class StateWebSocket
  {
    private HttpContext _context;
    private WebSocket _webSocket;
    System.Timers.Timer tmr;
    void InitTimer()
    {
      tmr = new System.Timers.Timer();
      tmr.Interval = 3000;
      tmr.AutoReset = false;
      tmr.Elapsed += OnElapsed;
      tmr.Enabled = true;
    }

    void OnElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      string replay = $"test state";
      var buffer = Encoding.UTF8.GetBytes(replay);

      _webSocket.SendAsync(
        new ArraySegment<byte>(buffer, 0, buffer.Length),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );

      // let timer start ticking
      tmr.Enabled = true;
    }

    public StateWebSocket(HttpContext context, WebSocket webSocket)
    {
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
  }
}
