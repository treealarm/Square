using Dapr.Client;
using Domain;
using Domain.OptionsModels;
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace PubSubLib
{
  public class PubService : IPubService
  {
    private DaprClient _client;
    private string _pubsub_name;

    public PubService(IOptions<DaprSettings>? daprSettings)
    {
      _pubsub_name = Environment.GetEnvironmentVariable("PUBSUB_NAME") ?? "";
      {
        Console.WriteLine($"PubService PUBSUB_NAME:{_pubsub_name}");
      }
      _client = new DaprClientBuilder().Build();
    }
    public async Task<long> Publish<T>(string channel, T message) where T : class
    {
      try
      {
        await _client.PublishEventAsync(_pubsub_name, channel, message);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return 0;
    }
  }
}
