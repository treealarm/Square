using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace DbLayer.Models
{
  [BsonIgnoreExtraElements]
  internal class DBObjectState
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public List<string> states { get; set; }
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
  }
}
