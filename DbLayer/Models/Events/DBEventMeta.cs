using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace DbLayer.Models
{
  internal class DBEventMeta
  {
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> extra_props { get; set; }
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> not_indexed_props { get; set; }
  }
}
