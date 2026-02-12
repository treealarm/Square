using System.Collections.Concurrent;

namespace BlinkService
{
  public class AlarmStateAccumulator : IAlarmStateAccumulator
  {
    private ConcurrentDictionary<string, AlarmActorState> _alarms = new();
    public void Publish(string id, AlarmActorState state)
    {
      _alarms[id] = state;
    }
    public Dictionary<string, AlarmActorState> Flush()
    {
      // создаём новый словарь
      var newDict = new ConcurrentDictionary<string, AlarmActorState>();

      // атомарно меняем ссылку
      var oldDict = Interlocked.Exchange(ref _alarms, newDict);

      // теперь oldDict больше никто не пишет
      return oldDict.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
  }
}
