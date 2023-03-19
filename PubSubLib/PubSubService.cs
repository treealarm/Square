using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace PubSubLib
{
  public class PubSubService: IPubSubService
  {    
    private string redisConnectionString = "localhost:6379";

    private object _locker = new object();
    private ConnectionMultiplexer redis;

    Dictionary<string, HashSet<Action<string, string>>> _topics =
      new Dictionary<string, HashSet<Action<string, string>>>();

    public PubSubService(IOptions<DaprSettings> daprSettings)
    {
      redisConnectionString = daprSettings.Value.reddis_endpoint;

      ConfigurationOptions configuration = new ConfigurationOptions();
      configuration.AbortOnConnectFail = false;
      configuration.EndPoints.Add(redisConnectionString);
      redis = ConnectionMultiplexer.Connect(configuration);
    }

    public void PublishNoWait(string channel, string message)
    {
      try
      {
        ISubscriber sub = redis.GetSubscriber();
        sub.PublishAsync(channel, message);
      }
      catch (Exception ex) { Console.WriteLine(ex.ToString()); }
    }

    public async Task<long> Publish(string channel, string message)
    {
      try
      {
        ISubscriber sub = redis.GetSubscriber();
        return await sub.PublishAsync(channel, message);
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return 0;
    }

    void RedisHandler(RedisChannel channel, RedisValue message)
    {
      List<Action<string, string>>? topicList = null;

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

    public async Task Subscribe(string channel, Action<string, string> handler)
    {
      int count = 0;

      lock (_locker)
      {
        HashSet<Action<string, string>>? topic = null;

        if (!_topics.TryGetValue(channel, out topic))
        {
          topic = new HashSet<Action<string, string>>();
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
          await subScriber.SubscribeAsync(channel, RedisHandler);
        }
        catch(Exception ex)
        { 
          Console.WriteLine(ex.ToString());
        }
      }      
    }

    public async Task Unsubscribe(string channel, Action<string, string> handler)
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
        await subScriber.UnsubscribeAsync(channel, RedisHandler);
      }            
    }
  }
}