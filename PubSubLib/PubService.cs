using Dapr.Client;
using Domain;

namespace PubSubLib
{
  public class PubService : IPubService
  {
    private DaprClient _client;
    protected string _pubsub_name;

    public PubService()
    {
      _pubsub_name = Environment.GetEnvironmentVariable("PUBSUB_NAME") ?? "";
      {
        Console.WriteLine($"PubService PUBSUB_NAME:{_pubsub_name}");
      }
      _client = new DaprClientBuilder().Build();
    }
    public async Task<long> Publish<T>(string pubsub_name, string channel, T message) where T : class
    {
      try
      {
        if (message is byte[] byteArray)
        {
          await _client.PublishByteEventAsync(
            pubsub_name, 
            channel,
            byteArray,
            "application/octet-stream"
          );
        }
        else
        {
          if (!await _client.CheckOutboundHealthAsync())
          {
            Console.WriteLine("Dapr sidecar not ready, retry later");
            return 0;
          }
          await _client.PublishEventAsync(pubsub_name, channel, message);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"pubsub_name={pubsub_name},channel={channel} -> {ex.ToString()}");
      }
      return 0;
    }
    public async Task<long> Publish<T>(string channel, T message) where T : class
    {      
      return await Publish(_pubsub_name, channel, message);
    }
  }
}
