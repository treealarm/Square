using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DbLayer
{
  [BsonIgnoreExtraElements]
  public class Marker
  {
#nullable enable
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? parent_id { get; set; }
#nullable disable

    [BsonElement("name")]
    public string name { get; set; } = null!;

    public double[] points { get; set; }
  }
}
