using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class HierarhyStateService : IHierarhyStateService
  {
    private IStateConsumer _stateConsumer;
    private IMapService _mapService;
    private IStateService _stateService;

    private Dictionary<string, AlarmObject> m_Hierarhy = new Dictionary<string, AlarmObject>();
    private object _locker = new object();
    public HierarhyStateService(
      IStateConsumer scService,
      IMapService mapService,
      IStateService stateService
    )
    {
      _stateConsumer = scService;
      _mapService = mapService;
      _stateService = stateService;
    }

    public async Task Init()
    {
      await Task.Delay(0);
    }

    private async Task BuildBranch(string id)
    {
      var obj = await _mapService.GetAsync(id);

      while (obj != null)
      {
        var alarmObject = new AlarmObject();
        obj.CopyAllTo(alarmObject);

        lock (_locker)
        {
          m_Hierarhy.Add(obj.id, alarmObject);

          if (string.IsNullOrEmpty(obj.parent_id) ||
            m_Hierarhy.ContainsKey(obj.parent_id))
          {
            break;
          }
        }

        obj = await _mapService.GetAsync(obj.parent_id);
      }
    }
    private async Task<List<AlarmObject>> SetAlarm(string id, bool alarm)
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
          return blinkChanges;
        }

        if (alarmObject.alarm == alarm)
        {
          return blinkChanges;
        }

        alarmObject.alarm = alarm;

        blinkChanges.Add(alarmObject);

        while (alarmObject != null)
        {
          if (
            string.IsNullOrEmpty(alarmObject.parent_id) ||
            !m_Hierarhy.TryGetValue(alarmObject.parent_id, out alarmObject))
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

      return blinkChanges;
    }

    public async Task OnStatesChanged(List<ObjectStateDTO> objStates)
    {
      List<AlarmObject> blinkChanges = new List<AlarmObject>();

      List<string> objIds = objStates.Select(el => el.id).ToList();
      var objsToUpdate = await _mapService.GetAsync(objIds);
      Dictionary<string, List<string>> mapExTypeToStates = new Dictionary<string, List<string>>();

      foreach (var objState in objStates)
      {
        var objToUpdate = objsToUpdate.Where(o => o.id == objState.id).FirstOrDefault();

        if (objToUpdate == null)
        {
          continue;
        }

        if (objToUpdate.external_type == null)
        {
          objToUpdate.external_type = string.Empty;
        }

        var stateDescrs = await _stateService
          .GetStateDescrAsync(objToUpdate.external_type, objState.states);

        var alarmedStateDescr = stateDescrs.Where(st => st.alarm).FirstOrDefault();

        var alarmedList = await SetAlarm(objToUpdate.id, alarmedStateDescr != null);
        blinkChanges.AddRange(alarmedList);
      }

      if (blinkChanges.Count > 0)
      {
        await _stateConsumer.OnBlinkStateChanged(blinkChanges);
      }
    }
  }
}
