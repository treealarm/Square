using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DbLayer.Models
{
  internal record DBGroup : BaseEntity
  {

    [BsonRepresentation(BsonType.ObjectId)]
    public string objid { get; set; }
    public string name { get; set; }
  }
}
