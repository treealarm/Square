using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  internal class DBObjExtraProperty
  {
    [BsonIgnoreIfNull]
    public string visual_type { get; set; }
    [BsonExtraElements]
    public BsonDocument str_val { get; set; }
    public string prop_name { get; set; }
  }

  internal class DBMarkerProperties
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> extra_props { get; set; }
  }
}
