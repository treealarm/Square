using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DbLayer
{
  [BsonIgnoreExtraElements]
  public class DBMarker
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string parent_id { get; set; }

    [BsonElement("name")]
    public string name { get; set; } = null;
  }
}
