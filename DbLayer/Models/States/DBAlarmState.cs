namespace DbLayer.Models
{
  internal record DBAlarmState : BasePgEntity
  {
    public bool alarm { get; set; }
  }
}
