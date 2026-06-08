using Dapr.Messaging.PublishSubscribe;
using Domain;

namespace PubSubLib
{
  public class SubService: ISubService, IDisposable
  {
    protected string _pubsub_name;
    private object _locker = new object();
    private readonly DaprPublishSubscribeClient _messagingClient;    

    private Dictionary<string, HashSet<MessageHandler>> _topics =
        new Dictionary<string, HashSet<MessageHandler>>();

    private CancellationTokenSource? _cancellationTokenSource;

    public SubService(DaprPublishSubscribeClient messagingClient)
    {
        _pubsub_name = EnvConfig.Require("PUBSUB_NAME");
        Console.WriteLine($"SubService PUBSUB_NAME:{_pubsub_name}");

      _messagingClient = messagingClient;
    }

    private static string BuildKey(string pubsub_name, string channel) => $"{pubsub_name}|{channel}";

    private static async Task InvokeHandlerSafe(MessageHandler handler, string channel, byte[] message)
    {
      try
      {
        await handler(channel, message);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    private Task<TopicResponseAction> OnMessage(string pubsub_name, string channel, byte[] message)
    {
      List<MessageHandler>? topicList = null;
      var key = BuildKey(pubsub_name, channel);

      lock (_locker)
      {
        if (_topics.TryGetValue(key, out var topic))
        {
          topicList = topic.ToList();
        }
      }

      if (topicList != null)
      {
        foreach (var action in topicList)
        {
          _ = InvokeHandlerSafe(action, channel, message);
        }
      }

      return Task.FromResult(TopicResponseAction.Success);
    }

    public async Task Subscribe(string pubsub_name, string channel, MessageHandler handler)
    {
      int count = 0;
      var key = BuildKey(pubsub_name, channel);
      CancellationToken token;

      lock (_locker)
      {
        // Первая подписка после старта или после того, как все отписались —
        // прежний CancellationTokenSource уже отменён, нужен новый.
        if (_topics.Count == 0)
        {
          _cancellationTokenSource?.Dispose();
          _cancellationTokenSource = new CancellationTokenSource();
        }

        if (!_topics.TryGetValue(key, out var topic))
        {
          topic = new HashSet<MessageHandler>();
          _topics.Add(key, topic);
        }

        topic.Add(handler);
        count = topic.Count;
        token = _cancellationTokenSource!.Token;
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
              token
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

    public Task Unsubscribe(string pubsub_name, string channel, MessageHandler handler)
    {
      var key = BuildKey(pubsub_name, channel);
      lock (_locker)
      {
        if (_topics.TryGetValue(key, out var topic))
        {
          topic.Remove(handler);
          if (topic.Count == 0)
          {
            _topics.Remove(key);
          }
        }
        if (_topics.Count == 0)
        {
          _cancellationTokenSource?.Cancel();
          _cancellationTokenSource?.Dispose();
          _cancellationTokenSource = null;
        }
      }
      return Task.CompletedTask;
    }
    public async Task Unsubscribe(string channel, MessageHandler handler)
    {
      await Unsubscribe(_pubsub_name, channel, handler);
    }

    public void Dispose()
    {
      lock (_locker)
      {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
      }
    }
  }
}