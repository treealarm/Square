namespace Domain.Events
{
  public class EventMetaDTO
  {
    public string id { get; set; } // unique event id
    public string object_id { get; set; } // Object id
    public string event_name { get; set; }
    public int event_priority { get; set; }
  }
}
