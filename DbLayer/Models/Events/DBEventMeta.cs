using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace DbLayer.Models.Events
{
  internal class DBEventMeta
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string id { get; set; } // unique event id

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string object_id { get; set; } // Object id
    public string event_name { get; set; }
    public int event_priority { get; set; }
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> extra_props { get; set; }
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> not_indexed_props { get; set; }
  }
}
