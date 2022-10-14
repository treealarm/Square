using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
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

namespace DbLayer.Services
{
  public class GeoService : IGeoService
  {
    private readonly IMongoCollection<BsonDocument> _geoRawCollection;
    private readonly IMongoCollection<DBGeoObject> _geoCollection;
    private readonly ILevelService _levelService;
    private readonly MongoClient _mongoClient;
    public GeoService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      ILevelService levelService
    )
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _levelService = levelService;

      var collName = geoStoreDatabaseSettings.Value.GeoCollectionName;

      _geoCollection = mongoDatabase.GetCollection<DBGeoObject>(collName);

      _geoRawCollection =
        mongoDatabase.GetCollection<BsonDocument>(collName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      IndexKeysDefinition<DBGeoObject> keys = "{ location: \"2dsphere\" }";
      var indexModel = new CreateIndexModel<DBGeoObject>(
        keys, new CreateIndexOptions()
        { Name = "location" }
      );

      _geoCollection.Indexes.CreateOneAsync(indexModel);
    }

    public async Task CreateOrUpdateGeoFromStringAsync(
      string id,
      string geometry,
      string type,
      string radius,
      string zoom_level
    )
    {
      if (string.IsNullOrEmpty(geometry))
      {
        return;
      }
      
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
        if (int.TryParse(radius, out var result))
          update = update.Set("radius", result);
      }

      if (string.IsNullOrEmpty(zoom_level))
      {
        zoom_level = null;
      }
      update = update.Set("zoom_level", zoom_level);

      var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
      var options = new UpdateOptions() { IsUpsert = true };
      var updateResult = await _geoRawCollection.UpdateOneAsync(filter, update, options);
    }

    private async Task<DBGeoObject> CreateOrUpdateGeoObject(DBGeoObject point)
    {
      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;
      var result = await _geoCollection.ReplaceOneAsync(x => x.id == point.id, point, opt);

      return point;
    }

    private async Task<DBGeoObject> CreateOrUpdateGeoPointAsync(FigureCircleDTO newObject)
    {
      DBGeoObject point = new DBGeoObject();

      point.zoom_level = newObject.zoom_level;
      point.radius = newObject.radius;

      point.location = ModelGate.ConvertGeoDTO2DB(newObject.geometry);

      point.id = newObject.id;
      
      return await CreateOrUpdateGeoObject(point);
    }

    private async Task<DBGeoObject> CreateOrUpdateGeoPolygonAsync(FigurePolygonDTO newObject)
    {
      DBGeoObject point = new DBGeoObject();
      point.zoom_level = newObject.zoom_level;
      point.location = ModelGate.ConvertGeoDTO2DB(newObject.geometry);

      point.id = newObject.id;
      return await CreateOrUpdateGeoObject(point);
    }

    private async Task<DBGeoObject> CreateOrUpdateGeoPolylineAsync(FigurePolylineDTO newObject)
    {
      DBGeoObject point = new DBGeoObject();

      point.location = ModelGate.ConvertGeoDTO2DB(newObject.geometry);
      point.zoom_level = newObject.zoom_level;
      point.id = newObject.id;
      return await CreateOrUpdateGeoObject(point);
    }

    public async Task<List<GeoObjectDTO>> GetGeoAsync(BoxDTO box)
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

      var levels = await _levelService.GetLevelsByZoom(box.zoom);

      var filter =
          builder.Where(p => levels.Contains(p.zoom_level)
          || string.IsNullOrEmpty(p.zoom_level))
        & builder.GeoIntersects(t => t.location, geometry);

      Log(filter);

      var list = await _geoCollection.Find(filter).ToListAsync();

      return ModelGate.ConvertListDB2DTO(list);
    }

    public async Task<List<GeoObjectDTO>> GetGeoObjectsAsync(List<string> ids)
    {
      List<DBGeoObject> obj = null;

      try
      {
        obj = await _geoCollection.Find(x => ids.Contains(x.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ModelGate.ConvertListDB2DTO(obj);
    }

    public async Task<GeoObjectDTO> GetGeoObjectAsync(string id)
    {
      DBGeoObject obj = null;

      try
      {
        obj = await _geoCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      }
      catch (Exception)
      {

      }

      return ModelGate.ConvertDB2DTO(obj);
    }

    public async Task<long> RemoveAsync(List<string> ids)
    {
      var result = await _geoCollection.DeleteManyAsync(x => ids.Contains(x.id));
      return result.DeletedCount;
    }

    public async Task<GeoObjectDTO> CreateGeo(FigureBaseDTO figure)
    {
      DBGeoObject geoPoint = null;

      if (figure is FigureCircleDTO circle)
      {
        geoPoint = await CreateOrUpdateGeoPointAsync(circle);
      }

      if (figure is FigurePolygonDTO polygon)
      {
        geoPoint = await CreateOrUpdateGeoPolygonAsync(polygon);
      }

      if (figure is FigurePolylineDTO polyline)
      {
        geoPoint = await CreateOrUpdateGeoPolylineAsync(polyline);
      }

      if (string.IsNullOrEmpty(figure.id))
      {
        // We could create figure first and then base object.
        figure.id = geoPoint.id;
      }

      return ModelGate.ConvertDB2DTO(geoPoint);
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
