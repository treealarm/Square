using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DbLayer
{
  [BsonIgnoreExtraElements]
  internal class DBMarker
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string parent_id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string owner_id { get; set; }// points to object which receive and own all states

    [BsonElement("name")]
    public string name { get; set; }
  }
}
