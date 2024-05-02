using Domain.StateWebSock;

namespace LogicMicroService
{
  public class TrackZonePosition
  {
    public TrackPointDTO Track { get; set; }
    public bool IsInZone { get; set; }
  }
}
