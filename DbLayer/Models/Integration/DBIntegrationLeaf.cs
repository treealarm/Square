using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DbLayer.Models
{
  [BsonIgnoreExtraElements]
  internal class DBIntegrationLeaf
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string integration_id { get; set; }
  }
}
