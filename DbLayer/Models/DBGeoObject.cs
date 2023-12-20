using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace DbLayer
{
  [BsonIgnoreExtraElements]
  internal class DBGeoObject
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public GeoJsonGeometry<GeoJson2DCoordinates> location { get; set; }

    [BsonIgnoreIfNull]
    public double? radius { get; set; }

    [BsonIgnoreIfNull]
    public string zoom_level { get; set; }
  }
}
