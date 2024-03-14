using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DbLayer.Models.Events
{
  [BsonIgnoreExtraElements]
  internal class DBEvent
  {
    public DBEventMeta meta { get; set; } = new DBEventMeta();
    public DateTime timestamp { get; set; }

    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> extra_props { get; set; }
  }
}
