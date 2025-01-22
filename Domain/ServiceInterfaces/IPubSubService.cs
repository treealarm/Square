using System;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public delegate Task MessageHandler(string channel, byte[] message);
  public interface ISubService
  {
    public Task Subscribe(string channel, MessageHandler handler);
    public Task Unsubscribe(string channel, MessageHandler handler);
  }
  public interface IPubService
  {
    public Task<long> Publish<T>(string channel, T message) where T : class;
  }
}
