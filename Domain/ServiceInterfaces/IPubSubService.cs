
using System.Threading.Tasks;

namespace Domain
{
  public delegate Task MessageHandler(string channel, byte[] message);
  public interface ISubService
  {
    public Task Subscribe(string channel, MessageHandler handler);
    public Task Subscribe(string pubsub_name, string channel, MessageHandler handler);
    public Task Unsubscribe(string channel, MessageHandler handler);
    public Task Unsubscribe(string pubsub_name, string channel, MessageHandler handler);
  }
  public interface IPubService
  {
    public Task<long> Publish<T>(string channel, T message) where T : class;
    public Task<long> Publish<T>(string pubsub_name, string channel, T message) where T : class;
  }
}
