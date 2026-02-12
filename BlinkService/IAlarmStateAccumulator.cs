namespace BlinkService
{
  public interface IAlarmStateAccumulator
  {
    void Publish(string id, bool alarmed);
    Dictionary<string, bool> Flush();
  }
}
