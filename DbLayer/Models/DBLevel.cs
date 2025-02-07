using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace DbLayer.Models
{
  internal class DBLevel
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }

    public string zoom_level { get; set; }
    public int zoom_min { get; set; }
    public int zoom_max { get; set; }
  }
}
