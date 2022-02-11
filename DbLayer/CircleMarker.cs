using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DbLayer
{
  public class CircleMarker
  {
#nullable enable
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
#nullable disable

    [BsonElement("Name")]
    public string MarkerName { get; set; } = null!;

    public double[] Center { get; set; }
  }
}
