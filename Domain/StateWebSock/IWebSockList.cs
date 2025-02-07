using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Domain
{
  public interface IWebSockList
  {
    Task PushAsync(object context, WebSocket webSocket);
  }
}
