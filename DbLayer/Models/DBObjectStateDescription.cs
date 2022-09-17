using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DbLayer.Models
{
  public class DBObjectStateDescription
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public bool alarm { get; set; }
    public string state { get; set; }
    public string state_descr { get; set; }
    public Color state_color { get; set; }
  }
}
