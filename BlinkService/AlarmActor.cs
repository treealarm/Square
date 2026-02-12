using BlinkService;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Security.Claims;
using System.Threading.Tasks;

// 1. Определяем интерфейс
public interface IAlarmActor : IActor
{
  Task SetAlarm(bool alarm);
  Task SetChildAlarmDelta(int delta);
}

// 2. State для Actor
[Serializable]
public class AlarmActorState
{
  public bool Alarm { get; set; } = false;         // реальная тревога
  public int ChildrenAlarms { get; set; } = 0;    // количество тревожных детей
  public string ParentId { get; set; } = null;    // родительский ActorId
}

// 3. Actor реализация
public class AlarmActor : Actor, IAlarmActor
{
  private const string StateName = "state";
  private AlarmActorState _state = default!;
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly IAlarmStateAccumulator _stateAcc;

  public AlarmActor(ActorHost host,
    IServiceScopeFactory scopeFactory,
    IAlarmStateAccumulator stateAcc) : base(host) 
  {
    _scopeFactory = scopeFactory;
    _stateAcc = stateAcc;
  }

  protected override async Task OnActivateAsync()
  {
    if (await StateManager.ContainsStateAsync(StateName))
    {
      _state = await StateManager.GetStateAsync<AlarmActorState>(StateName);
    }
    else
    {
      _state = new AlarmActorState();
    }

    if (string.IsNullOrEmpty(_state.ParentId)) 
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        var mapService = scope.ServiceProvider.GetRequiredService<IMapService>();
        var me = await mapService.GetAsync(this.Id.ToString());
        _state.ParentId = me.parent_id;
      }

      await StateManager.SetStateAsync(StateName, _state);
    }

    await base.OnActivateAsync();
  }

  public async Task SetAlarm(bool alarm)
  {
    if (_state.Alarm == alarm)
      return;

    _stateAcc.Publish(Id.ToString(), alarm);

    bool oldAlarmed = IsAlarmed();

    _state.Alarm = alarm;

    await StateManager.SetStateAsync(StateName, _state);

    bool newAlarmed = IsAlarmed();

    if (oldAlarmed != newAlarmed)
      await NotifyParent(alarm ? 1 : -1);
  }

  public async Task SetChildAlarmDelta(int delta)
  {
    bool oldAlarmed = IsAlarmed();

    _state.ChildrenAlarms += delta;

    await StateManager.SetStateAsync(StateName, _state);

    bool newAlarmed = IsAlarmed();

    if (oldAlarmed != newAlarmed)
    {
      await NotifyParent(delta);
      _stateAcc.Publish(Id.ToString(), newAlarmed);
    }      
  }

  private bool IsAlarmed()
      => _state.Alarm || _state.ChildrenAlarms > 0;

  private async Task NotifyParent(int delta)
  {
    if (string.IsNullOrEmpty(_state.ParentId))
      return;

    var parent = ActorProxy.Create<IAlarmActor>(
        new ActorId(_state.ParentId),
        nameof(AlarmActor)
    );

    await parent.SetChildAlarmDelta(delta);
  }
}

