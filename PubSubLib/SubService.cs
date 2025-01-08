using Dapr.Messaging.PublishSubscribe;
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using System.Collections.Concurrent;
using System.Text;

namespace PubSubLib
{
  public class SubService: ISubService, IDisposable
  {
    private string _pubsub_name;
    private object _locker = new object();
    private readonly DaprPublishSubscribeClient _messagingClient;

    private Dictionary<string, HashSet<Func<string, string, Task>>> _topics =
      new Dictionary<string, HashSet<Func<string, string, Task>>>();

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

    private async Task<TopicResponseAction> OnMessage(string channel, string message)
    {
      TopicResponseAction retVal = TopicResponseAction.Success;
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
        await Task.Run(() =>
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
      return retVal;
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
          async Task<TopicResponseAction> HandleMessageAsync(TopicMessage message, CancellationToken cancellationToken)
          {
            try
            {              
              var data = Encoding.UTF8.GetString(message.Data.Span);
              return await OnMessage(message.Topic, data);
            }
            catch
            {
              return TopicResponseAction.Retry;
            }
          }

          // Создаем подписку
         
          await _messagingClient.SubscribeAsync(
              _pubsub_name,
              channel,
              new DaprSubscriptionOptions(new MessageHandlingPolicy(TimeSpan.FromSeconds(10), TopicResponseAction.Retry)),
              HandleMessageAsync,
              _cancellationTokenSource.Token
          );
        }
        catch(Exception ex)
        { 
          Console.WriteLine(ex.ToString());
        }
      }      
    }

    public async Task Unsubscribe(string channel, Func<string, string, Task> handler)
    {
      await Task.Delay(0);
      int count = 0;

      lock (_locker)
      {
        if (_topics.TryGetValue(channel, out var topic))
        {
          topic.Remove(handler);
          count = topic.Count;
        }
        if (count == 0)
        {
          _topics.Remove(channel);
        }
        if (_topics.Count == 0)
        {
          _cancellationTokenSource.Cancel();
        }
      }

                 
    }

    public void Dispose()
    {

    }
  }
}