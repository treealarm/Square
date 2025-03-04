using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DbLayer.Models
{
  internal class DBEvent
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; } // unique event id
    public DateTime timestamp { get; set; }
    
    public Guid object_id { get; set; } // Object id
    public string event_name { get; set; }
    public int event_priority { get; set; }
    public List<PgDBObjExtraProperty> extra_props { get; set; }
  }
}
