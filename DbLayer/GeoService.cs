using Domain;
using Domain.GeoDTO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbLayer
{
  public class GeoService
  {
    private readonly IMongoCollection<BsonDocument> _geoRawCollection;
    private readonly IMongoCollection<GeoPoint> _geoCollection;
    public GeoService(
      IMongoCollection<GeoPoint> geoCollection,
      IMongoCollection<BsonDocument> geoRawCollection
    )
    {
      _geoCollection = geoCollection;
      _geoRawCollection = geoRawCollection;
    }

    public async Task CreateOrUpdateGeoFromStringAsync(
      string id,
      string geometry,
      string type,
      string radius,
      string min_zoom,
      string max_zoom
    )
    {
      BsonDocument doc = new BsonDocument();
      BsonArray arr = new BsonArray();

      if (type == GeoJsonObjectType.Point.ToString())
      {
        arr = BsonSerializer.Deserialize<BsonValue>(geometry).AsBsonArray;

        var temp = arr[0];
        arr[0] = arr[1];
        arr[1] = temp;
      }
      else
      {
        var val = BsonSerializer.Deserialize<BsonValue>(geometry).AsBsonArray;

        foreach (var element in val)
        {
          var temp = element[0];
          element[0] = element[1];
          element[1] = temp;
        }

        if (type == GeoJsonObjectType.Polygon.ToString())
        {
          val.Add(val[0]);
          arr.Add(val);
        }

        if (type == GeoJsonObjectType.LineString.ToString())
        {
          arr = val;
        }
      }

      doc.Add("coordinates", arr);
      doc.Add("type", type);

      var update = Builders<BsonDocument>.Update.Set("location", doc);

      if (!string.IsNullOrEmpty(radius))
      {
        update = update.Set("radius", radius);
      }

      if (!string.IsNullOrEmpty(min_zoom))
      {
        update = update.Set("min_zoom", min_zoom);
      }

      if (!string.IsNullOrEmpty(max_zoom))
      {
        update = update.Set("max_zoom", max_zoom);
      }

      var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
      var options = new UpdateOptions() { IsUpsert = true };
      var updateResult = await _geoRawCollection.UpdateOneAsync(filter, update, options);
    }


    public async Task<GeoPoint> CreateOrUpdateGeoPointAsync(FigureCircleDTO newObject)
    {
      GeoPoint point = new GeoPoint();
      point.radius = newObject.radius;

      point.location = new GeoJsonPoint<GeoJson2DCoordinates>(
        GeoJson.Position(newObject.geometry[1], newObject.geometry[0])
      );
      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(point);
      }

      return point;
    }

    public async Task<GeoPoint> CreateOrUpdateGeoPolygonAsync(FigurePolygonDTO newObject)
    {
      GeoPoint point = new GeoPoint();

      List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

      for (int i = 0; i < newObject.geometry.Length; i++)
      {
        coordinates.Add(GeoJson.Position(newObject.geometry[i][1], newObject.geometry[i][0]));
      }

      coordinates.Add(GeoJson.Position(newObject.geometry[0][1], newObject.geometry[0][0]));

      point.location = GeoJson.Polygon(coordinates.ToArray());

      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(point);
      }

      return point;
    }

    public async Task<GeoPoint> CreateOrUpdateGeoPolylineAsync(FigurePolylineDTO newObject)
    {
      GeoPoint point = new GeoPoint();

      List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

      for (int i = 0; i < newObject.geometry.Length; i++)
      {
        coordinates.Add(GeoJson.Position(newObject.geometry[i][1], newObject.geometry[i][0]));
      }

      point.location = GeoJson.LineString(coordinates.ToArray());

      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(point);
      }

      return point;
    }

    public async Task<List<GeoPoint>> GetGeoAsync(BoxDTO box)
    {
      var builder = Builders<GeoPoint>.Filter;
      var geometry = GeoJson.Polygon(
        new GeoJson2DCoordinates[]
        {
          GeoJson.Position(box.wn[0], box.wn[1]),
          GeoJson.Position(box.es[0], box.wn[1]),
          GeoJson.Position(box.es[0], box.es[1]),
          GeoJson.Position(box.wn[0], box.es[1]),
          GeoJson.Position(box.wn[0], box.wn[1])
        }
      );

      var filter = builder.GeoIntersects(t => t.location, geometry);
      //var filter = builder.GeoWithinBox(t => t.location, box.wn[0], box.wn[1], box.es[0], box.es[1]);
      Log(filter);
      var list = await _geoCollection.Find(filter).ToListAsync();

      return list;
    }

    public async Task<List<GeoPoint>> GetGeoObjectsAsync(List<string> ids)
    {
      List<GeoPoint> obj = null;

      try
      {
        obj = await _geoCollection.Find(x => ids.Contains(x.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return obj;
    }

    public async Task<GeoPoint> GetGeoObjectAsync(string id)
    {
      GeoPoint obj = null;

      try
      {
        obj = await _geoCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      }
      catch (Exception)
      {

      }

      return obj;
    }

    public async Task<DeleteResult> RemoveAsync(List<string> ids)
    {
      //var idsFilter = Builders<Marker>.Filter.In(d => d.id, ids);
      //return await _circleCollection.DeleteManyAsync(idsFilter);
      return await _geoCollection.DeleteManyAsync(x => ids.Contains(x.id));
    }

    private static void Log(FilterDefinition<GeoPoint> filter)
    {
      var serializerRegistry = BsonSerializer.SerializerRegistry;
      var documentSerializer = serializerRegistry.GetSerializer<GeoPoint>();
      var rendered = filter.Render(documentSerializer, serializerRegistry);
      Console.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
      Console.WriteLine();
    }
  }
}
