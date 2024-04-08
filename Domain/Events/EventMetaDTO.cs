using System.Collections.Generic;

namespace Domain.Events
{
  public class EventMetaDTO
  {
    public string id { get; set; } // unique event id
    public string object_id { get; set; } // Object id
    public string event_name { get; set; }
    public int event_priority { get; set; }
    public List<ObjExtraPropertyDTO> extra_props { get; set; }
    public List<ObjExtraPropertyDTO> not_indexed_props { get; set; }
  }
}
