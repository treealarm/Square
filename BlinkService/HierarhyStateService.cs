using Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace BlinkService
{
  internal class HierarhyStateService : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, AlarmObject> _hierarhy = new();
    private readonly HashSet<string> _idsUpdated = new();

    public HierarhyStateService(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    private async Task CheckStatesByIds(string channel, byte[] message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null || ids.Count == 0)
        return;

      lock (_idsUpdated)
        _idsUpdated.UnionWith(ids);

      await Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      using var scope = _serviceProvider.CreateScope();
      var sub = scope.ServiceProvider.GetRequiredService<ISubService>();
      var stateService = scope.ServiceProvider.GetRequiredService<IStateService>();
      var stateUpdateService = scope.ServiceProvider.GetRequiredService<IStatesUpdateService>();

      await stateUpdateService.DropStateAlarms();
      var initialAlarmedStates = await stateService.GetAlarmedStates(null);
      await sub.Subscribe(Topics.CheckStatesByIds, CheckStatesByIds);

      const int maxProcess = 10000;
      for (int i = 0; i < initialAlarmedStates.Count; i += maxProcess)
      {
        var batch = initialAlarmedStates.Values.Skip(i).Take(maxProcess).ToList();
        await ProcessStates(batch, scope.ServiceProvider);
      }

      _timer = Task.Run(() => DoWork(), _cancellationToken.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      using var scope = _serviceProvider.CreateScope();
      var sub = scope.ServiceProvider.GetRequiredService<ISubService>();
      await sub.Unsubscribe(Topics.CheckStatesByIds, CheckStatesByIds);

      _cancellationToken.Cancel();
      if (_timer != null)
        await _timer;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }

    private AlarmObject GetAlarmObjectFromCache(string id)
    {
      _hierarhy.TryGetValue(id, out var alarmObject);
      return alarmObject;
    }

    private AlarmObject GetOrCreateAlarmObject(BaseMarkerDTO marker)
    {
      if (marker == null)
        return null;

      if (_hierarhy.TryGetValue(marker.id, out var existing))
        return existing;

      var alarmObject = new AlarmObject();
      marker.CopyAllTo(alarmObject);
      _hierarhy[alarmObject.id] = alarmObject;

      lock (_idsUpdated)
        _idsUpdated.Add(alarmObject.id);

      return alarmObject;
    }

    private async Task<List<AlarmObject>> SetAlarm(BaseMarkerDTO objToUpdate, bool alarm, IServiceProvider provider)
    {
      var alarmObject = GetOrCreateAlarmObject(objToUpdate);
      var blinkChanges = new List<AlarmObject>();

      if (alarmObject == null || alarmObject.alarm == alarm)
        return blinkChanges;

      alarmObject.alarm = alarm;
      blinkChanges.Add(alarmObject);

      while (!string.IsNullOrEmpty(alarmObject?.parent_id))
      {
        var parent_id = alarmObject.parent_id;
        alarmObject = GetAlarmObjectFromCache(parent_id);

        if (alarmObject == null)
        {
          var mapService = provider.GetRequiredService<IMapService>();
          var parent = await mapService.GetAsync(parent_id);
          alarmObject = GetOrCreateAlarmObject(parent);
        }

        if (alarmObject == null)
          break;

        if (alarm)
        {
          if (alarmObject.children_alarms == 0)
            blinkChanges.Add(alarmObject);
          alarmObject.children_alarms++;
        }
        else
        {
          alarmObject.children_alarms--;
          if (alarmObject.children_alarms == 0)
            blinkChanges.Add(alarmObject);
        }
      }

      return blinkChanges;
    }

    private async Task ProcessStates(List<ObjectStateDTO> objStates, IServiceProvider provider)
    {
      var blinkChanges = new List<AlarmObject>();

      var mapService = provider.GetRequiredService<IMapService>();
      var objsToUpdate = await mapService.GetAsync(objStates.Select(i => i.id).ToList());

      var allStates = objStates.SelectMany(s => s.states).ToHashSet();
      var stateService = provider.GetRequiredService<IStateService>();
      var alarmedStateDescr = await stateService.GetAlarmStatesDescr(allStates.ToList());

      foreach (var objState in objStates)
      {
        if (!objsToUpdate.TryGetValue(objState.id, out var objToUpdate) || objToUpdate == null)
          continue;

        bool isAlarmed = objState.states.Any(s => alarmedStateDescr.ContainsKey(s));
        var alarmedList = await SetAlarm(objToUpdate, isAlarmed, provider);
        blinkChanges.AddRange(alarmedList);
      }

      if (blinkChanges.Count > 0)
      {
        var stateUpdateService = provider.GetRequiredService<IStatesUpdateService>();
        await stateUpdateService.UpdateAlarmStatesAsync(blinkChanges.Select(t => new AlarmState()
        {
          id = t.id,
          alarm = t.alarm || t.children_alarms > 0
        }).ToList());
      }
    }

    private async Task ProcessIds(List<string> objIds, IServiceProvider provider)
    {
      var stateService = provider.GetRequiredService<IStateService>();
      var objStates = await stateService.GetStatesAsync(objIds);
      await ProcessStates(objStates.Values.ToList(), provider);
    }

    private async Task DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        List<string> objIds;
        lock (_idsUpdated)
        {
          objIds = _idsUpdated.ToList();
          _idsUpdated.Clear();
        }

        if (objIds.Count == 0)
        {
          await Task.Delay(1000);
          continue;
        }

        try
        {
          using var scope = _serviceProvider.CreateScope();
          var provider = scope.ServiceProvider;

          const int maxProcess = 10000;
          for (int i = 0; i < objIds.Count; i += maxProcess)
          {
            var batch = objIds.Skip(i).Take(maxProcess).ToList();
            await ProcessIds(batch, provider);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }
  }
}
