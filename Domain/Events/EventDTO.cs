using System;
using System.Collections.Generic;

namespace Domain.Events
{
  public class EventDTO
  {
    public EventMetaDTO meta { get; set; } = new EventMetaDTO();
    public DateTime timestamp { get; set; }
    public List<ObjExtraPropertyDTO> extra_props { get; set; }
  }
}
