using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Models
{
  [BsonIgnoreExtraElements]
  public class DBRoutLine
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public DBGeoObject figure { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string id_start { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string id_end { get; set; }
  }
}
