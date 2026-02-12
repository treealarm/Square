namespace DbLayer.Models
{
  internal record DBAlarmState : BasePgEntity
  {
    public bool? alarm { get; set; }
    public int children_alarms { get; set; }
  }
}
