using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class HierarhyStateService : IHierarhyStateService
  {
    private IStateConsumer _stateConsumer;
    IMapService _mapService;
    private Dictionary<string, AlarmObject> m_Hierarhy = new Dictionary<string, AlarmObject>();
    private object _locker = new object();
    public HierarhyStateService(
      IStateConsumer scService,
      IMapService mapService
    )
    {
      _stateConsumer = scService;
      _mapService = mapService;
    }

    public async Task Init()
    {
      await Task.Delay(0);
    }

    private async Task BuildBranch(string id)
    {      
      var obj = await _mapService.GetAsync(id);

      while (obj != null && !string.IsNullOrEmpty(obj.parent_id))
      {
        var alarmObject = new AlarmObject();
        obj.CopyAllTo(alarmObject);

        lock (_locker)
        {
          m_Hierarhy.Add(obj.id, alarmObject);

          if (m_Hierarhy.ContainsKey(obj.parent_id))
          {
            break;
          }
        }
        
        obj = await _mapService.GetAsync(obj.parent_id);
      }
    }
    public async Task SetAlarm(string id, bool alarm)
    {
      AlarmObject alarmObject;
      List<AlarmObject> blinkChanges = new List<AlarmObject>();

      lock (_locker)
      {
        m_Hierarhy.TryGetValue(id, out alarmObject);
      }

      if (alarmObject == null)
      {
        await BuildBranch(id);
      }

      lock (_locker)
      {
        if (!m_Hierarhy.TryGetValue(id, out alarmObject))
        {
          return;
        }

        if (alarmObject.alarm == alarm)
        {
          return;
        }

        blinkChanges.Add(alarmObject);

        while (alarmObject != null)
        {
          if (!m_Hierarhy.TryGetValue(alarmObject.parent_id, out alarmObject))
          {
            break;
          }

          // Here is parent.
          if (alarm)
          {
            if (alarmObject.children_alarms == 0)
            {
              blinkChanges.Add(alarmObject);
            }

            alarmObject.children_alarms++;
          }
          else
          {
            alarmObject.children_alarms--;

            if (alarmObject.children_alarms == 0)
            {
              blinkChanges.Add(alarmObject);
            }
          }
        }
      }

      if (blinkChanges.Count > 0)
      {
        await _stateConsumer.OnBlinkStateChanged(blinkChanges);
      }
    }
  }
}
