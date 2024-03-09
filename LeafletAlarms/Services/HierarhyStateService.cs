using Dapr;
using DbLayer;
using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.Extensions.Hosting;
using PubSubLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class HierarhyStateService : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private IPubSubService _pubsub;
    private IMapService _mapService;
    private IStateService _stateService;

    private Dictionary<string, AlarmObject> m_Hierarhy = new Dictionary<string, AlarmObject>();
    private ConcurrentDictionary<string,string> _idsUpdated = new ConcurrentDictionary<string,string>();
    public HierarhyStateService(
      IPubSubService pubsub,
      IMapService mapService,
      IStateService stateService
    )
    {
      _pubsub = pubsub;
      _mapService = mapService;
      _stateService = stateService;      
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


    private async Task ProcessIds(List<string> objIds)
    {
      List<AlarmObject> blinkChanges = new List<AlarmObject>();

      var objStates = await _stateService.GetStatesAsync(objIds);
      var objsToUpdate = await _mapService.GetAsync(objIds);

      Dictionary<string, List<string>> mapExTypeToStates = new Dictionary<string, List<string>>();

      foreach (var objState in objStates)
      {
        BaseMarkerDTO objToUpdate = null;
        objsToUpdate.TryGetValue(objState.id, out objToUpdate);

        if (objToUpdate == null)
        {
          continue;
        }

        if (objToUpdate.external_type == null)
        {
          objToUpdate.external_type = string.Empty;
        }

        ObjectStateDescriptionDTO alarmedStateDescr = null;

        if (objState.states.Count > 0)
        {
          var stateDescrs = await _stateService
                    .GetStateDescrAsync(objToUpdate.external_type, objState.states);
          alarmedStateDescr = stateDescrs.Where(st => st.alarm).FirstOrDefault();
        }

        var alarmedList = await SetAlarm(objToUpdate, alarmedStateDescr != null);
        blinkChanges.AddRange(alarmedList);
      }

      if (blinkChanges.Count > 0)
      {
        _pubsub.PublishNoWait(Topics.OnBlinkStateChanged, JsonSerializer.Serialize(blinkChanges));
      }
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      _pubsub.Subscribe(Topics.CheckStatesByIds, CheckStatesByIds);
      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();

      return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      _pubsub.Unsubscribe(Topics.CheckStatesByIds, CheckStatesByIds);
      _cancellationToken.Cancel();
      _timer?.Wait();

      return Task.CompletedTask;
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
