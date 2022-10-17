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
    public DBRoutLineMeta meta { get; set; } = new DBRoutLineMeta();

    [BsonRepresentation(BsonType.ObjectId)]
    public string id_start { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string id_end { get; set; }

    public DateTime? ts_start { get; set; }
    public DateTime? ts_end { get; set; }
    public int processed { get; set; } = 0;
  }
}
