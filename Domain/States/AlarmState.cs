namespace Domain
{
  public class AlarmBaseState
  {
    public string? id { get; set; }
    public bool? alarm { get; set; }
  }
  public class AlarmState: AlarmBaseState
  {
    public int children_alarms { get; set; }
  }
}
