using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Threading.Tasks;

// 1. Определяем интерфейс
public interface IAlarmActor : IActor
{
  Task SetAlarm(bool alarm);
  Task SetChildAlarmDelta(int delta);
  Task<AlarmActorState> GetState();
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
  private const string StateName = "statestore";

  public AlarmActor(ActorHost host) : base(host) { }

  protected override async Task OnActivateAsync()
  {
    await StateManager.SetStateAsync(StateName, new AlarmActorState());
    Console.WriteLine($"Activating actor id: {this.Id}");
    await base.OnActivateAsync();
  }
  protected override async Task OnDeactivateAsync()
  {
    // Provides Opporunity to perform optional cleanup.
    Console.WriteLine($"Deactivating actor id: {this.Id}");
    await base.OnDeactivateAsync();
  }
  // 4. Установка собственной тревоги
  public async Task SetAlarm(bool alarm)
  {
    var state = await StateManager.GetStateAsync<AlarmActorState>(StateName);
    if (state.Alarm != alarm)
    {
      state.Alarm = alarm;
      await StateManager.SetStateAsync(StateName, state);

      // Уведомляем родителя
      if (!string.IsNullOrEmpty(state.ParentId))
      {
        var parentActor = ActorProxy.Create<IAlarmActor>(
            new ActorId(state.ParentId),
            "AlarmActor"
        );

        await parentActor.SetChildAlarmDelta(alarm ? 1 : -1);
      }
    }
  }

  // 5. Изменение счётчика тревожных детей
  public async Task SetChildAlarmDelta(int delta)
  {
    var state = await StateManager.GetStateAsync<AlarmActorState>(StateName);
    int oldChildrenAlarms = state.ChildrenAlarms;
    state.ChildrenAlarms += delta;

    await StateManager.SetStateAsync(StateName, state);

    // Если суммарная тревога изменилась, уведомляем родителя
    bool oldAlarmed = oldChildrenAlarms > 0 || state.Alarm;
    bool newAlarmed = state.ChildrenAlarms > 0 || state.Alarm;

    if (oldAlarmed != newAlarmed && !string.IsNullOrEmpty(state.ParentId))
    {
      var parentActor = ActorProxy.Create<IAlarmActor>(
            new ActorId(state.ParentId),
            "AlarmActor"
        );
      await parentActor.SetChildAlarmDelta(delta);
    }
  }

  // 6. Получение состояния Actor
  public async Task<AlarmActorState> GetState()
  {
    return await StateManager.GetStateAsync<AlarmActorState>(StateName);
  }
}
