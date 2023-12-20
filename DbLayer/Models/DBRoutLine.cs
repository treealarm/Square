using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace DbLayer.Models
{
  [BsonIgnoreExtraElements]
  internal class DBRoutLine
  {
    public DBFigTsMeta meta { get; set; } = new DBFigTsMeta();

    [BsonRepresentation(BsonType.ObjectId)]
    public string id_start { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string id_end { get; set; }

    public DateTime? ts_start { get; set; }
    public DateTime? ts_end { get; set; }
  }
}
