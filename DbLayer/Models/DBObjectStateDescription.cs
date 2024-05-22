using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbLayer.Models
{
  internal class DBObjectStateDescription
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public bool alarm { get; set; }
    public string state { get; set; }
    public string state_descr { get; set; }
    public string state_color { get; set; }
    [BsonIgnoreIfNull]
    public string external_type { get; set; }
  }
}
