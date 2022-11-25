using Domain.StateWebSock;

namespace LeafletAlarms.Services.Logic
{
  public class TrackZonePosition
  {
    public TrackPointDTO Track { get; set; }
    public bool IsInZone { get; set; }
  }
}
