using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Diagram;

namespace DbLayer.Models.Diagrams
{
  internal class DBDiagram
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string dgr_type { get; set; }
    [BsonIgnoreIfNull]
    public DBDiagramCoord geometry { get; set; }
  }
}
