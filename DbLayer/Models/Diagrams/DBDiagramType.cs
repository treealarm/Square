using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Models.Diagrams
{
  internal class DBDiagramType
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string name { get; set; }
    public string src { get; set; }
    [BsonIgnoreIfNull]
    public List<DBDiagramTypeRegion> regions { get; set; }
  }
}
