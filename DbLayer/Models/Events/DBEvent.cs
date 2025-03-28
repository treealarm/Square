using System;
using System.Collections.Generic;


namespace DbLayer.Models
{
  internal record DBEvent: BasePgEntity
  {
    public DateTime timestamp { get; set; }
    
    public Guid object_id { get; set; } // Object id
    public string event_name { get; set; }
    public int event_priority { get; set; }
    public List<PgDBObjExtraProperty> extra_props { get; set; }
  }
}
