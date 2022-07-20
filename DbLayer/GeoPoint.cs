using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace DbLayer
{
  public class GeoPoint
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public GeoJsonGeometry<GeoJson2DCoordinates> location { get; set; }
    public double radius { get; set; }
    public int max_zoom { get; set; }
    public int min_zoom { get; set; }
  }
}
