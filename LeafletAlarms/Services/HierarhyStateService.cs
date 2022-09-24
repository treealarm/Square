using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class HierarhyStateService : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationToken _cancellationToken = new CancellationToken();

    private DateTime _lastUpdate = DateTime.UtcNow;

    private IStateConsumer _stateConsumer;
    private IMapService _mapService;
    private IStateService _stateService;
    private readonly IIdsQueue _stateIdsQueueService;

    private Dictionary<string, AlarmObject> m_Hierarhy = new Dictionary<string, AlarmObject>();

    public HierarhyStateService(
      IStateConsumer scService,
      IMapService mapService,
      IStateService stateService,
      IIdsQueue stateIdsQueueService
    )
    {
      _stateConsumer = scService;
      _mapService = mapService;
      _stateService = stateService;
      _stateIdsQueueService = stateIdsQueueService;
    }

    public async Task Init()
    {
      await Task.Delay(0);
    }

    private async Task<AlarmObject> GetAlarmObject(string id)
    {
      AlarmObject alarmObject = null;

      if (m_Hierarhy.TryGetValue(id, out alarmObject))
      {
        return alarmObject;
      }

      var marker = await _mapService.GetAsync(id);

      if (marker == null)
      {
        return null;
      }

      alarmObject = new AlarmObject();
      marker.CopyAllTo(alarmObject);

      m_Hierarhy.Add(alarmObject.id, alarmObject);        

      // Add Id for new object, So if it is alarmed we support actual state.
      _stateIdsQueueService.AddId(alarmObject.id);

      return alarmObject;
    }

    private async Task<List<AlarmObject>> SetAlarm(string id, bool alarm)
    {
      AlarmObject alarmObject = await GetAlarmObject(id);
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

        alarmObject = await GetAlarmObject(alarmObject.parent_id);

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

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      _timer = new Task(() => DoWork(), _cancellationToken);
      _timer.Start();

      return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
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
        List<string> objIds = _stateIdsQueueService.GetIds();

        if (objIds.Count == 0)
        {
          await Task.Delay(1000);
          continue;

          // Code to update states from db.
          //var newStates = await _stateService.GetStatesByTimeAsync(_lastUpdate, 100);

          //if (newStates.Count == 0)
          //{
          //  await Task.Delay(1000);
          //  continue;
          //}

          //objIds = newStates.Select(el => el.id).ToList();

          //var lastState = newStates.MaxBy(el => el.timestamp);

          //if (lastState != null)
          //{
          //  _lastUpdate = lastState.timestamp;
          //}
          //else
          //{
          //  _lastUpdate = DateTime.UtcNow;
          //}
        }
        
        await ProcessIds(objIds);
        continue;
      }
    }
  }
}
