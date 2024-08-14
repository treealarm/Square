using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DbLayer.Models
{
  internal class DBDiagram
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string dgr_type { get; set; }
    [BsonIgnoreIfNull]
    public DBDiagramCoord geometry { get; set; }
    public string region_id { get; set; }
    public string background_img { get; set; }
    
  }
}
