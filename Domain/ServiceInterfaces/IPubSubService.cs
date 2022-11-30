using System;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IPubSubService
  {
    public Task<long> Publish(string channel, string message);
    public Task Subscribe(string channel, Action<string, string> handler);
    public Task Unsubscribe(string channel, Action<string, string> handler);
  }
}
