namespace Domain
{
  public record AlarmBaseState: BaseObjectDTO
  {
    public bool? alarm { get; set; }
  }
  public record AlarmState: AlarmBaseState
  {
    public int children_alarms { get; set; }
  }
}
