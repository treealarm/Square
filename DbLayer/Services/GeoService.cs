using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.OptionsModels;
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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DbLayer.Services
{
    public class GeoService : IGeoService
  {
    private readonly IMongoCollection<BsonDocument> _geoRawCollection;
    private readonly IMongoCollection<DBGeoObject> _geoCollection;
    private readonly ILevelService _levelService;
    private readonly IMongoClient _mongoClient;
    public GeoService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      ILevelService levelService,
      IMongoClient mongoClient
    )
    {
      _mongoClient = mongoClient;

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
      {
        IndexKeysDefinition<DBGeoObject> keys =
          new IndexKeysDefinitionBuilder<DBGeoObject>()
          .Geo2DSphere(d => d.location)
          .Ascending(d => d.zoom_level)
          ;

        var indexModel = new CreateIndexModel<DBGeoObject>(
          keys, new CreateIndexOptions()
          { Name = "combi" }
        );

        _geoCollection.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task CreateOrUpdateGeoFromStringAsync(
      string id,
      string geometry,
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

      GeometryDTO geo = JsonSerializer.Deserialize<GeometryDTO>(geometry);
      var type = geo.type;
      
      if (type == GeoJsonObjectType.Point.ToString())
      {
        GeometryCircleDTO fig = geo as GeometryCircleDTO;
        arr = new BsonArray(fig.coord);

        //arr = BsonSerializer.Deserialize<BsonValue>(geometry).AsBsonArray;

        var temp = arr[0];
        arr[0] = arr[1];
        arr[1] = temp;
      }
      else
      {
        BsonArray val;

        if (type == GeoJsonObjectType.Polygon.ToString())
        {
          var fig = geo as GeometryPolygonDTO;
          val = new BsonArray(fig.coord);
        }
        else
        {
          var fig = geo as GeometryPolylineDTO;
          val = new BsonArray(fig.coord);
        }

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

    public async Task<Dictionary<string, GeoObjectDTO>> GetGeoAsync(BoxDTO box)
    {
      int limit = 10000;

      if (box.count != null && box.count > 0)
      {
        limit = box.count.Value;
      }

      var builder = Builders<DBGeoObject>.Filter;

      GeoJsonGeometry<GeoJson2DCoordinates> geometry;
      FilterDefinition<DBGeoObject> filter = null;

      if (box.zone != null)
      {
        foreach (var zone in box.zone)
        {
          geometry = ModelGate.ConvertGeoDTO2DB(zone);
          var f1 = builder.GeoIntersects(t => t.location, geometry);

          if (filter == null)
          {
            filter = f1;
          }
          else
          {
            filter = filter | f1;
          }
        }
      }
      else
      {
        geometry = GeoJson.Polygon(
          new GeoJson2DCoordinates[]
          {
                  GeoJson.Position(box.wn[0], box.wn[1]),
                  GeoJson.Position(box.es[0], box.wn[1]),
                  GeoJson.Position(box.es[0], box.es[1]),
                  GeoJson.Position(box.wn[0], box.es[1]),
                  GeoJson.Position(box.wn[0], box.wn[1])
          }
        );

        filter = builder.GeoIntersects(t => t.location, geometry);
      }

      if (box.not_in_zone)
      {
        filter = builder.Not(filter);
      }

      var levels = await _levelService.GetLevelsByZoom(box.zoom);

      if (box.zoom != null)
      {
        levels.Add(null);
        levels.Add(string.Empty);
        filter = filter
        &
        builder.Where(p => levels.Contains(p.zoom_level))
        ;
      }      

      if (box.ids != null &&
        (box.ids.Count > 0 || 
        (box.property_filter != null && box.property_filter.props.Count > 0))
      )
      {
        filter = filter & builder.Where(t => box.ids.Contains(t.id));
      }

      //Log(filter);

      try
      {
        var list = await _geoCollection.Find(filter)
        .Limit(limit)
        .ToListAsync();

        return ModelGate.ConvertListDB2DTO(list);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return new Dictionary<string, GeoObjectDTO>();
    }

    public async Task<Dictionary<string, GeoObjectDTO>> GetGeoIntersectAsync(
      GeometryDTO geoObject,
      HashSet<string> ids,
      bool bNot
    )
    {
      int limit = 10000;

      var builder = Builders<DBGeoObject>.Filter;

      GeoJsonGeometry<GeoJson2DCoordinates> geometry;
      FilterDefinition<DBGeoObject> filter = null;

      geometry = ModelGate.ConvertGeoDTO2DB(geoObject);
      filter = builder.GeoIntersects(t => t.location, geometry);      

      if (bNot)
      {
        filter = builder.Not(filter);
      }

      if (ids != null && ids.Any())
      {
        filter = filter & builder.Where(t => ids.Contains(t.id));
      }

      try
      {
        var list = await _geoCollection.Find(filter)
          .Limit(limit)
          .ToListAsync();

        return ModelGate.ConvertListDB2DTO(list);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return new Dictionary<string, GeoObjectDTO>();
    }

    public async Task<Dictionary<string, GeoObjectDTO>> GetGeoObjectsAsync(List<string> ids)
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

    public async Task<Dictionary<string, GeoObjectDTO>> CreateGeo(IEnumerable<FigureGeoDTO> figures)
    {
      var retVal = new Dictionary<string, GeoObjectDTO>();

      if (!figures.Any())
      {
        return retVal;
      }

      var bulkWrites = new List<WriteModel<DBGeoObject>>();
      var dbUpdated = new Dictionary<FigureGeoDTO, DBGeoObject>();

      foreach (var figure in figures)
      {
        DBGeoObject dbObj = new DBGeoObject();

        dbObj.zoom_level = figure.zoom_level;
        dbObj.radius = figure.radius;

        dbObj.location = ModelGate.ConvertGeoDTO2DB(figure.geometry);

        dbObj.id = figure.id;
        dbUpdated.Add(figure, dbObj);

        var filter = Builders<DBGeoObject>.Filter.Eq(x => x.id, dbObj.id);

        if (string.IsNullOrEmpty(dbObj.id))
        {
          var request = new InsertOneModel<DBGeoObject>(dbObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBGeoObject>(filter, dbObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        } 
      }

      if (!bulkWrites.Any())
      {
        return retVal;
      }

      var writeResult = await _geoCollection.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
        retVal[pair.Value.id] = ModelGate.ConvertDB2DTO(pair.Value);
      }

      return retVal ;
    }

    public async Task<Dictionary<string, GeoObjectDTO>> GetGeoObjectNearestsAsync(
      List<string> ids,
      Geo2DCoordDTO ptDto,
      int limit
    )
    {
      var point = new GeoJsonPoint<GeoJson2DCoordinates>(
          GeoJson.Position(ptDto.Lon, ptDto.Lat)
        );
      //var point = GeoJson.Point(GeoJson.Position(-74.005, 40.7358879));

      List<DBGeoObject> obj = null;

      try
      {
        var maxGeoDistanceInKm = 1;
        var builder = Builders<DBGeoObject>.Filter;
        var filter0 = builder.Where(x => ids.Contains(x.id));
        var filter1 = builder.NearSphere(x => x.location, point, maxGeoDistanceInKm * 1000);
        var filter = filter0 & filter1;
        obj = await _geoCollection.Find(filter).Limit(limit).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ModelGate.ConvertListDB2DTO(obj);
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
