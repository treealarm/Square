using System;

namespace Domain.Events
{
  public class EventDTO
  {
    public EventMetaDTO meta { get; set; } = new EventMetaDTO();
    public DateTime timestamp { get; set; }
    public string id { get; set; } = default!;// unique event id
    public string object_id { get; set; } = default!;// Object id
    public string event_name { get; set; } = default!;
    public int event_priority { get; set; }
  }
}
