using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;


namespace DbLayer.Models
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
