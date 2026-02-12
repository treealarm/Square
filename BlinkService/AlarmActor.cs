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
  public bool? Alarm { get; set; } = null;         // реальная тревога
  public int ChildrenAlarms { get; set; } = 0;    // количество тревожных детей
  public string ParentId { get; set; } = null;    // родительский ActorId

  public AlarmActorState Clone()
  {
    return new AlarmActorState
    {
      Alarm = this.Alarm,
      ChildrenAlarms = this.ChildrenAlarms,
      ParentId = this.ParentId
    };
  }

}

// 3. Actor реализация
public class AlarmActor : Actor, IAlarmActor
{
  private AlarmActorState _state = new();
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
    using (var scope = _scopeFactory.CreateScope())
    {
      var my_id = this.Id.ToString();

      var mapService = scope.ServiceProvider.GetRequiredService<IMapService>();
      var me = await mapService.GetAsync(my_id);
      _state.ParentId = me.parent_id;

      var stateService = scope.ServiceProvider.GetRequiredService<IStateService>();
      var alarm_states = await stateService.GetAlarmStatesAsync(new List<string> { my_id });

      if (alarm_states.TryGetValue(my_id, out var alarm_state_value) && alarm_state_value != null)
      {
        _state.ChildrenAlarms = alarm_state_value.children_alarms;
        _state.Alarm = alarm_state_value.alarm;
      }
    }

    await base.OnActivateAsync();
  }

  public async Task SetAlarm(bool alarm)
  {
    if (_state.Alarm == alarm)
      return;    

    bool oldAlarmed = IsAlarmed();

    _state.Alarm = alarm;

    _stateAcc.Publish(Id.ToString(), _state.Clone());

    bool newAlarmed = IsAlarmed();

    if (oldAlarmed != newAlarmed)
      await NotifyParent(alarm ? 1 : -1);
  }

  public async Task SetChildAlarmDelta(int delta)
  {
    bool oldAlarmed = IsAlarmed();

    _state.ChildrenAlarms += delta;

    bool newAlarmed = IsAlarmed();

    if (oldAlarmed != newAlarmed)
    {
      await NotifyParent(delta);
      _stateAcc.Publish(Id.ToString(), _state.Clone());
    }      
  }

  private bool IsAlarmed()
      => _state.Alarm == true || _state.ChildrenAlarms > 0;

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

