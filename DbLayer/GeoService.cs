using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace DbLayer
{
  public class GeoService
  {
    private readonly IMongoCollection<BsonDocument> _geoRawCollection;
    private readonly IMongoCollection<DBGeoObject> _geoCollection;
    private readonly MapService _parent;
    public GeoService(
      MapService parent,
      IMongoCollection<DBGeoObject> geoCollection,
      IMongoCollection<BsonDocument> geoRawCollection
    )
    {
      _parent = parent;
      _geoCollection = geoCollection;
      _geoRawCollection = geoRawCollection;
    }

    public async Task CreateOrUpdateGeoFromStringAsync(
      string id,
      string geometry,
      string type,
      string radius,
      string zoom_level
    )
    {
      BsonDocument doc = new BsonDocument();
      BsonArray arr = new BsonArray();

      if (type == GeoJsonObjectType.Point.ToString())
      {
        GeometryCircleDTO fig = JsonSerializer.Deserialize<GeometryCircleDTO>(geometry);
        arr = new BsonArray(fig.coord);
        
        //arr = BsonSerializer.Deserialize<BsonValue>(geometry).AsBsonArray;

        var temp = arr[0];
        arr[0] = arr[1];
        arr[1] = temp;
      }
      else
      {
        GeometryPolygonDTO fig = JsonSerializer.Deserialize<GeometryPolygonDTO>(geometry);
        var val = new BsonArray(fig.coord);
        //var val = BsonSerializer.Deserialize<BsonValue>(geometry).AsBsonArray;

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
        if (Int32.TryParse(radius, out var result))
          update = update.Set("radius", result);
      }

      if (!string.IsNullOrEmpty(zoom_level))
      {
          update = update.Set("zoom_level", zoom_level);
      }


      var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
      var options = new UpdateOptions() { IsUpsert = true };
      var updateResult = await _geoRawCollection.UpdateOneAsync(filter, update, options);
    }


    public async Task<DBGeoObject> CreateOrUpdateGeoPointAsync(FigureCircleDTO newObject)
    {
      DBGeoObject point = new DBGeoObject();
      point.radius = newObject.radius;

      point.location = ModelGate.ConvertGeoDTO2DB(newObject.geometry);

      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(point);
      }

      return point;
    }

    public async Task<DBGeoObject> CreateOrUpdateGeoPolygonAsync(FigurePolygonDTO newObject)
    {
      DBGeoObject point = new DBGeoObject();

      point.location = ModelGate.ConvertGeoDTO2DB(newObject.geometry);

      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(point);
      }

      return point;
    }

    public async Task<DBGeoObject> CreateOrUpdateGeoPolylineAsync(FigurePolylineDTO newObject)
    {
      DBGeoObject point = new DBGeoObject();

      point.location = ModelGate.ConvertGeoDTO2DB(newObject.geometry);

      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(point);
      }

      return point;
    }

    public async Task<List<DBGeoObject>> GetGeoAsync(BoxDTO box)
    {
      var builder = Builders<DBGeoObject>.Filter;
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

      var levels = await _parent.LevelServ.GetLevelsByZoom(box.zoom);

      var filter =
          builder.Where(p => levels.Contains(p.zoom_level))
        & builder.GeoIntersects(t => t.location, geometry);

      Log(filter);
      var list = await _geoCollection.Find(filter).ToListAsync();

      return list;
    }

    public async Task<List<DBGeoObject>> GetGeoObjectsAsync(List<string> ids)
    {
      List<DBGeoObject> obj = null;

      try
      {
        obj = await _geoCollection.Find(x => ids.Contains(x.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return obj;
    }

    public async Task<DBGeoObject> GetGeoObjectAsync(string id)
    {
      DBGeoObject obj = null;

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

    private static void Log(FilterDefinition<DBGeoObject> filter)
    {
      var serializerRegistry = BsonSerializer.SerializerRegistry;
      var documentSerializer = serializerRegistry.GetSerializer<DBGeoObject>();
      var rendered = filter.Render(documentSerializer, serializerRegistry);
      Debug.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
      Debug.WriteLine("");
    }
  }
}
