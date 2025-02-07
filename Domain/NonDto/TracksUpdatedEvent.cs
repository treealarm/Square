using System;

namespace Domain
{
  public class TracksUpdatedEvent
  {
    public DateTime? ts_start { get; set; }
    public DateTime? ts_end { get; set; }
    public string? id_start { get; set; }
    public string? id_end { get; set; }
  }
}
