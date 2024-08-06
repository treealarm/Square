using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DbLayer.Models.Integration
{
  internal class DBIntegration
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string parent_id { get; set; }

    [BsonElement("name")]
    public string name { get; set; }
  }
}
