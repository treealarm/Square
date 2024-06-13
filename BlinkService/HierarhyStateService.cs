using DataChangeLayer;
using Domain;
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Domain.States;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BlinkService
{
  public class HierarhyStateService : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private readonly IPubService _pub;
    private readonly ISubService _sub;
    private readonly IMapService _mapService;
    private readonly IStateService _stateService;
    private readonly IStatesUpdateService _stateUpdateService;

    private Dictionary<string, AlarmObject> m_Hierarhy = new Dictionary<string, AlarmObject>();
    private ConcurrentDictionary<string,string> _idsUpdated = new ConcurrentDictionary<string,string>();
    public HierarhyStateService(
      IPubService pub,
      ISubService sub,
      IMapService mapService,
      IStateService stateService,
      IStatesUpdateService stateUpdateService
    )
    {
      _pub = pub;
      _sub = sub;
      _mapService = mapService;
      _stateService = stateService;   
      _stateUpdateService = stateUpdateService;
    }

    public async Task Init()
    {
      await Task.Delay(0);
    }

    private AlarmObject GetAlarmObjectFromCash(string id)
    {
      AlarmObject alarmObject = null;
      m_Hierarhy.TryGetValue(id, out alarmObject);
      return alarmObject;
    }

    private AlarmObject GetAlarmObject(BaseMarkerDTO marker)
    {
      if (marker == null)
      {
        return null;
      }

      AlarmObject alarmObject = GetAlarmObjectFromCash(marker.id);

      if (alarmObject != null)
      {
        return alarmObject;
      }     

      alarmObject = new AlarmObject();
      marker.CopyAllTo(alarmObject);

      m_Hierarhy.Add(alarmObject.id, alarmObject);        

      // Add Id for new object, So if it is alarmed we support actual state.
      _idsUpdated.TryAdd(alarmObject.id, string.Empty);
      return alarmObject;
    }

    private async Task<List<AlarmObject>> SetAlarm(BaseMarkerDTO objToUpdate, bool alarm)
    {
      AlarmObject alarmObject = GetAlarmObject(objToUpdate);
      List<AlarmObject> blinkChanges = new List<AlarmObject>();

      if (alarmObject == null)
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
        if (string.IsNullOrEmpty(alarmObject.parent_id))
        {
          break;
        }

        var parent_id = alarmObject.parent_id;

        alarmObject = GetAlarmObjectFromCash(parent_id);

        if (alarmObject == null)
        {
          BaseMarkerDTO parent = await _mapService.GetAsync(parent_id);

          alarmObject = GetAlarmObject(parent);
        }

        if (alarmObject == null)
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

      return blinkChanges;
    }

    private async Task ProcessStates(List<ObjectStateDTO> objStates)
    {
      List<AlarmObject> blinkChanges = new List<AlarmObject>();
      var objsToUpdate = await _mapService.GetAsync(objStates.Select(i => i.id).ToList());
      var allStates = new HashSet<string>();
      foreach (var objState in objStates)
      {
        allStates.UnionWith(objState.states);
      }

      var alarmedStateDescr = await _stateService
                    .GetAlarmStatesDescr(allStates.ToList());

      foreach (var objState in objStates)
      {
        BaseMarkerDTO objToUpdate = null;
        objsToUpdate.TryGetValue(objState.id, out objToUpdate);

        if (objToUpdate == null)
        {
          continue;
        }

        bool isAlarmed = objState.states.Any(s => alarmedStateDescr.ContainsKey(s));
        
        var alarmedList = await SetAlarm(objToUpdate, isAlarmed);
        blinkChanges.AddRange(alarmedList);
      }

      if (blinkChanges.Count > 0)
      {
        // Write alarm to DB/
        await _stateUpdateService.UpdateAlarmStatesAsync(
          blinkChanges.Select(t => new AlarmState()
          {
            id = t.id,
            alarm = t.alarm || t.children_alarms > 0
          }).ToList()
          );
      }
    }
    private async Task ProcessIds(List<string> objIds)
    {
      var objStates = await _stateService.GetStatesAsync(objIds);

      await ProcessStates(objStates);
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      await _stateUpdateService.DropStateAlarms();
      var initialAlarmedStates = await _stateService.GetAlarmedStates(null);
      await _sub.Subscribe(Topics.CheckStatesByIds, CheckStatesByIds);

      int maxProcess = 10000;

      for (int i = 0; i < initialAlarmedStates.Count; i += maxProcess)
      {
        await ProcessStates(initialAlarmedStates.Skip(i).Take(maxProcess).ToList());
      }

      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();

      //return Task.CompletedTask;
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      await _sub.Unsubscribe(Topics.CheckStatesByIds, CheckStatesByIds);
      _cancellationToken.Cancel();
      _timer?.Wait();

      //return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }

    private async void DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        List<string> objIds = _idsUpdated.Keys.ToList();

        if (objIds.Count == 0)
        {
          await Task.Delay(1000);
          continue;
        }

        foreach (var key in objIds)
        {
          string val;
          if (!_idsUpdated.TryRemove(key, out val))
          {
            Console.WriteLine("TryRemove error");
          }
        }

        try
        {
          int maxProcess = 10000;

          for (int i = 0; i < objIds.Count; i += maxProcess)
          {
            await ProcessIds(objIds.Skip(i).Take(maxProcess).ToList());
          }          
        }
        catch(Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }

    async Task CheckStatesByIds(string channel, string message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null)
      {
        return;
      }

      foreach(var id in ids)
      {
        _idsUpdated.TryAdd(id, string.Empty);
      }      
      await Task.CompletedTask;
    }
  }
}
