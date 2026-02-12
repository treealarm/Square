namespace BlinkService
{
  public interface IAlarmStateAccumulator
  {
    void Publish(string id, AlarmActorState state);
    Dictionary<string, AlarmActorState> Flush();
  }
}
