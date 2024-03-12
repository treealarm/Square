using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace PubSubLib
{
  public class PubSubService: IPubSubService
  {  

    private string redisConnectionString = "localhost:6379";

    private object _locker = new object();
    private ConnectionMultiplexer redis;

    private Dictionary<string, HashSet<Func<string, string, Task>>> _topics =
      new Dictionary<string, HashSet<Func<string, string, Task>>>();

    private static ConcurrentDictionary<string, RedisChannel> _channels 
      = new ConcurrentDictionary<string, RedisChannel>();

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
    public PubSubService(IOptions<DaprSettings> daprSettings)
    {
      redisConnectionString = daprSettings.Value.reddis_endpoint;

      ConfigurationOptions configuration = new ConfigurationOptions();
      configuration.AbortOnConnectFail = false;
      configuration.EndPoints.Add(redisConnectionString);
      redis = ConnectionMultiplexer.Connect(configuration);
    }

    public async Task<long> Publish(string channel, string message)
    {
      try
      {
        ISubscriber sub = redis.GetSubscriber();
        return await sub.PublishAsync(GetLiteralChannel(channel), message, CommandFlags.FireAndForget);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return 0;
    }

    private void RedisHandler(RedisChannel channel, RedisValue message)
    {
      List<Func<string, string, Task>>? topicList = null;

      lock (_locker)
      {
        if (_topics.TryGetValue(channel.ToString(), out var topic))
        {
          topicList = topic.ToList();
        }
      }

      if (topicList != null)
      {
        Task.Run(() =>
        {
          var sChan = channel.ToString();
          var sMsg = message.ToString();

          foreach (var action in topicList)
          {
            try
            {
              action(sChan, sMsg);
            }
            catch(Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
          }
        });
      }
    }

    public async Task Subscribe(string channel, Func<string, string, Task> handler)
    {
      int count = 0;

      lock (_locker)
      {
        HashSet<Func<string, string,Task>>? topic = null;

        if (!_topics.TryGetValue(channel, out topic))
        {
          topic = new HashSet<Func<string, string, Task>>();
          _topics.Add(channel, topic);
        }

        topic.Add(handler);
        count = topic.Count;
      }

      if (count == 1)
      {
        try
        {
          ISubscriber subScriber = redis.GetSubscriber();
          await subScriber.SubscribeAsync(GetLiteralChannel(channel), RedisHandler);
        }
        catch(Exception ex)
        { 
          Console.WriteLine(ex.ToString());
        }
      }      
    }

    public async Task Unsubscribe(string channel, Func<string, string, Task> handler)
    {
      int count = 0;

      lock (_locker)
      {
        if (_topics.TryGetValue(channel, out var topic))
        {
          topic.Remove(handler);
          count = topic.Count;
        }
      }

      if (count == 0)
      {
        ISubscriber subScriber = redis.GetSubscriber();
        await subScriber.UnsubscribeAsync(GetLiteralChannel(channel), RedisHandler);
      }            
    }
  }
}