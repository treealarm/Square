﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace DbLayer
{
  [BsonIgnoreExtraElements]
  public class DBGeoObject
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public GeoJsonGeometry<GeoJson2DCoordinates> location { get; set; }
    [BsonIgnoreIfNull]
    public double? radius { get; set; }
    public string zoom_level { get; set; }
  }
}