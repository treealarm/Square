using MongoDB.Bson;
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
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string id { get; set; } // unique event id

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string object_id { get; set; } // Object id
    public string event_name { get; set; }
    public int event_priority { get; set; }
  }
}
