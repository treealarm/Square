using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public interface IWebSockList
  {
    Task PushAsync(object context, WebSocket webSocket);
  }
}
