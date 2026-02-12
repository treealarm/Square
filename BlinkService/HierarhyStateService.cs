using Domain;
using System.Text.Json;
using Dapr.Actors.Client;
using Dapr.Actors;

namespace BlinkService
{
  internal class HierarhyStateService : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private readonly IServiceProvider _serviceProvider;

    private readonly IActorProxyFactory _actorProxy;
    private readonly IAlarmStateAccumulator _stateAcc;

    public HierarhyStateService(
      IServiceProvider serviceProvider, 
      IActorProxyFactory actorProxy,
      IAlarmStateAccumulator stateAcc)
    {
      _serviceProvider = serviceProvider;
      _actorProxy = actorProxy;
      _stateAcc = stateAcc;
    }


    private async Task AlarmStatesChanged(string channel, byte[] message)
    {
      var alarmStatesUpdated = JsonSerializer.Deserialize<List<AlarmState>>(message);

      if (alarmStatesUpdated == null || alarmStatesUpdated.Count == 0)
        return;

      try
      {
        foreach(var alarmState in alarmStatesUpdated)
        {
          var nodeActor = _actorProxy.CreateActorProxy<IAlarmActor>(
           new ActorId(alarmState.id),
           nameof(AlarmActor)
          );
          await nodeActor.SetAlarm(alarmState.alarm == true);
        }
        
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

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

      await sub.Subscribe(Topics.AlarmStatesChanged, AlarmStatesChanged);

      _timer = Task.Run(() => DoWork(), _cancellationToken.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      using var scope = _serviceProvider.CreateScope();
      var sub = scope.ServiceProvider.GetRequiredService<ISubService>();

      await sub.Subscribe(Topics.AlarmStatesChanged, AlarmStatesChanged);

      _cancellationToken.Cancel();
      if (_timer != null)
        await _timer;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }

    private async Task<bool> ProcessStates(IServiceScope scope)
    {
      var blinkChanges = _stateAcc.Flush();

      if (blinkChanges.Count > 0)
      {
        var stateUpdateService = scope.ServiceProvider.GetRequiredService<IStatesUpdateService>();
        await stateUpdateService.UpdateAlarmStatesAsync(blinkChanges.Select(t => new AlarmState()
        {
          id = t.Key,
          alarm = t.Value
        }).ToList());
      }

      return blinkChanges.Count > 0;
    }

    private async Task DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        try
        {
          using var scope = _serviceProvider.CreateScope();
          var isNewChanges = await ProcessStates(scope);

          if (!isNewChanges)
          {
            await Task.Delay(1000);
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
