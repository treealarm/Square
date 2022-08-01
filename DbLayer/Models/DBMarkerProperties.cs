using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public class DBObjExtraProperty
  {
    public string visual_type { get; set; }
    [BsonExtraElements]
    public BsonDocument MetaValue { get; set; }
    public string prop_name { get; set; }
  }

  public class DBMarkerProperties
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> extra_props { get; set; }
  }
}
