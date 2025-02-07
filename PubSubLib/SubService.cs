using Dapr.Messaging.PublishSubscribe;
using Domain;
using System.Collections.Concurrent;

namespace PubSubLib
{
  public class SubService: ISubService, IDisposable
  {
    protected string _pubsub_name;
    private object _locker = new object();
    private readonly DaprPublishSubscribeClient _messagingClient;    

    private Dictionary<string, HashSet<MessageHandler>> _topics =
        new Dictionary<string, HashSet<MessageHandler>>();


  private static ConcurrentDictionary<string, string> _channels 
      = new ConcurrentDictionary<string, string>();
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public SubService(DaprPublishSubscribeClient messagingClient)
    {
        _pubsub_name = Environment.GetEnvironmentVariable("PUBSUB_NAME") ?? "";
        {
          Console.WriteLine($"SubService PUBSUB_NAME:{_pubsub_name}");
        }
      
      _messagingClient = messagingClient;
    }

    private async Task<TopicResponseAction> OnMessage(string pubsub_name, string channel, byte[] message)
    {
      TopicResponseAction retVal = TopicResponseAction.Success;
      List<MessageHandler>? topicList = null;
      var key = pubsub_name + channel;

      lock (_locker)
      {
        if (_topics.TryGetValue(key, out var topic))
        {
          topicList = topic.ToList();
        }
      }

      if (topicList != null)
      {
        await Task.Run(() =>
        {
          var sChan = channel.ToString();

          foreach (var action in topicList)
          {
            try
            {
              action(sChan, message);
            }
            catch(Exception ex)
            {
              Console.WriteLine(ex.Message);
            }
          }
        });
      }
      return retVal;
    }

    public async Task Subscribe(string pubsub_name, string channel, MessageHandler handler)
    {
      int count = 0;
      var key = pubsub_name + channel;

      lock (_locker)
      {
        HashSet<MessageHandler>? topic = null;

        if (!_topics.TryGetValue(key, out topic))
        {
          topic = new HashSet<MessageHandler>();
          _topics.Add(key, topic);
        }

        topic.Add(handler);
        count = topic.Count;
      }

      if (count == 1)
      {
        try
        {
          async Task<TopicResponseAction> HandleMessageAsync(TopicMessage message, CancellationToken cancellationToken)
          {
            try
            {
              //var data = Encoding.UTF8.GetString(message.Data.Span);
              return await OnMessage(message.PubSubName, message.Topic, message.Data.Span.ToArray());
            }
            catch
            {
              return TopicResponseAction.Retry;
            }
          }

          // Создаем подписку

          await _messagingClient.SubscribeAsync(
              pubsub_name,
              channel,
              new DaprSubscriptionOptions(new MessageHandlingPolicy(TimeSpan.FromSeconds(10), TopicResponseAction.Retry)),
              HandleMessageAsync,
              _cancellationTokenSource.Token
          );
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }
    public async Task Subscribe(string channel, MessageHandler handler)
    {
         await Subscribe(_pubsub_name, channel, handler);
    }

    public async Task Unsubscribe(string pubsub_name, string channel, MessageHandler handler)
    {
      await Task.Delay(0);
      int count = 0;
      var key = pubsub_name + channel;
      lock (_locker)
      {
        if (_topics.TryGetValue(key, out var topic))
        {
          topic.Remove(handler);
          count = topic.Count;
        }
        if (count == 0)
        {
          _topics.Remove(key);
        }
        if (_topics.Count == 0)
        {
          _cancellationTokenSource.Cancel();
        }
      }
    }
    public async Task Unsubscribe(string channel, MessageHandler handler)
    {
      await Unsubscribe(_pubsub_name, channel, handler);
    }

    public void Dispose()
    {

    }
  }
}