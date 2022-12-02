using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Models
{
  internal class DB__FAKE
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string logic_id { get; set; }
    public DBGeoObject figure { get; set; }
  }
}
