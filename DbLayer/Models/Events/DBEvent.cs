using System;
using System.Collections.Generic;


namespace DbLayer.Models
{
  internal record EventProp : BasePgEntity
  {
    public string prop_name { get; set; }
    public string str_val { get; set; }
    public string visual_type { get; set; }
    public Guid owner_id { get; set; }
  }

  internal record DBEvent: BasePgEntity
  {
    public DateTime timestamp { get; set; }
    
    public Guid object_id { get; set; } // Object id
    public string event_name { get; set; }
    public string param0 { get; set; }
    public string param1 { get; set; }
    public int event_priority { get; set; }
    public List<EventProp> extra_props { get; set; }
  }
}
