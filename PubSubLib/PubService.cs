using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PubSubLib
{
  public class PubService : IPubService
  {
    private string? redisConnectionString;

    private ConnectionMultiplexer _redis;
    private static ConcurrentDictionary<string, RedisChannel> _channels
      = new ConcurrentDictionary<string, RedisChannel>();
    public PubService(IOptions<DaprSettings>? daprSettings)
    {
      redisConnectionString = daprSettings?.Value.reddis_endpoint;

      ConfigurationOptions configuration = new ConfigurationOptions();
      configuration.AbortOnConnectFail = false;
      configuration.EndPoints.Add(redisConnectionString??string.Empty);
      _redis = ConnectionMultiplexer.Connect(configuration);
    }

    private static RedisChannel GetLiteralChannel(string channel)
    {
      RedisChannel redisChan;

      if (!_channels.TryGetValue(channel, out redisChan))
      {
        redisChan = RedisChannel.Literal(channel);
        _channels.TryAdd(channel, redisChan);
      }
      return redisChan;
    }
    public async Task<long> Publish<T>(string channel, T message) where T : class
    {
      try
      {
        ISubscriber sub = _redis.GetSubscriber();
        return await sub.PublishAsync(
          GetLiteralChannel(channel),
          JsonSerializer.Serialize(message),
          CommandFlags.FireAndForget);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return 0;
    }
  }
}
