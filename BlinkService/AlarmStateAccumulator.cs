using System.Collections.Concurrent;

namespace BlinkService
{
  public class AlarmStateAccumulator : IAlarmStateAccumulator
  {
    private ConcurrentDictionary<string, bool> _alarms = new();
    public void Publish(string id, bool alarmed)
    {
      _alarms[id] = alarmed;
    }
    public Dictionary<string, bool> Flush()
    {
      // создаём новый словарь
      var newDict = new ConcurrentDictionary<string, bool>();

      // атомарно меняем ссылку
      var oldDict = Interlocked.Exchange(ref _alarms, newDict);

      // теперь oldDict больше никто не пишет
      return oldDict.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
  }
}
