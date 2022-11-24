using System;
using System.Collections.Generic;
using System.Linq;

namespace LeafletAlarms.Services
{
  public class PubSubService
  {
    private object _locker = new object();

    Dictionary<string, HashSet<Action<string, object>>> _topics = 
      new Dictionary<string, HashSet<Action<string, object>>>();

    public long Publish(string channel, object message)
    {
      List<Action<string, object>> topicList = null;

      lock (_locker)
      {
        if (_topics.TryGetValue(channel, out var topic))
        {
          topicList = topic.ToList();          
        }
      }

      if (topicList != null)
      {
        foreach (var action in topicList)
        {
          action(channel, message);
        }
        return topicList.Count;
      }      

      return 0;
    }

    public void Subscribe(string channel, Action<string, object> handler)
    {
      lock (_locker)
      {
        if (!_topics.ContainsKey(channel))
        {
          _topics[channel] = new HashSet<Action<string, object>>();
        }
        _topics[channel].Add(handler);
      }      
    }

    public void Unsubscribe(string channel, Action<string, object> handler)
    {
      lock (_locker)
      {
        if (_topics.TryGetValue(channel, out var topic))
        {
          topic.Remove(handler);
        }
      }
    }
  }
}
