using System;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ISubService
  {
    public Task Subscribe(string channel, Func<string, string, Task> handler);
    public Task Unsubscribe(string channel, Func<string, string, Task> handler);
  }
  public interface IPubService
  {
    public Task<long> Publish<T>(string channel, T message) where T : class;
  }
}
