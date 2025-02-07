using System;

namespace Domain
{
  public class EventDTO
  {
    public EventMetaDTO meta { get; set; } = new EventMetaDTO();
    public DateTime timestamp { get; set; }
    public string? id { get; set; }// unique event id
    public string? object_id { get; set; }// Object id
    public string? event_name { get; set; }
    public int event_priority { get; set; }
  }
}
