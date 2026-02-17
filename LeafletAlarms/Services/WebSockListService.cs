using Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

namespace LeafletAlarms.Services
{
  public class WebSockListService : IWebSockList
  {
    private IServiceProvider _serviceProvider;
    private ISubService _sub;
    private IPubService _pub;

    public WebSockListService(
      IServiceProvider provider,
      ISubService sub,
      IPubService pub
    )
    {
      _serviceProvider = provider;
      _sub = sub;
      _pub = pub;
    }

    ~WebSockListService()
    {
    }
    public static ConcurrentDictionary<string, StateWebSocket> StateSockets { get; set; } =
      new ConcurrentDictionary<string, StateWebSocket>();
    public async Task PushAsync(object context, WebSocket webSocket)
    {
      using var scope = _serviceProvider.CreateScope();

      var stateWs = scope.ServiceProvider.GetRequiredService<StateWebSocket>();
      var con = context as HttpContext;

      stateWs.Init(context as HttpContext, webSocket);

      StateSockets.TryAdd(con.Connection.Id, stateWs);

      try
      {
        await stateWs.ProcessAcceptedSocket();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error occurred while processing WebSocket. {ex}");
      }
      finally
      {
        try
        {
          if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.CloseSent)
          {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.InternalServerError,
                "An error occurred",
                CancellationToken.None
            );
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error occurred while closing WebSocket.{ex.Message}");
        }
        finally
        {
          StateSockets.TryRemove(con.Connection.Id, out _);
        }
      }
    }

  }
}
