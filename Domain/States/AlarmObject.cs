
namespace Domain
{
  public record AlarmObject: BaseMarkerDTO
  {
    public bool alarm { get; set; }
    public int children_alarms { get; set; }
  }
}
