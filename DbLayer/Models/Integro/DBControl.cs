using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace DbLayer.Models
{
  internal record DBControl : BaseEntity
  {
    [BsonRepresentation(BsonType.ObjectId)]
    public string owner_id { get; set; }

    public string name { get; set; }

    public string control_type { get; set; }

    [BsonIgnoreIfNull]
    public BsonValue value { get; set; }

    public IDictionary<string, object> parameters { get; set; }
  }
}
