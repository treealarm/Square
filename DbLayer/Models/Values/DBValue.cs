using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DbLayer.Models
{
  internal record DBValue : BaseEntity
  {

    [BsonRepresentation(BsonType.ObjectId)]
    public string owner_id { get; set; }
    public string name { get; set; }
    [BsonIgnoreIfNull]
    public BsonValue value { get; set; }
  }
}
