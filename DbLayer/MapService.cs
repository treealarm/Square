using Domain;
using Domain.GeoDTO;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DbLayer
{
  public class MapService
  {
    private readonly IMongoCollection<Marker> _circleCollection;
    private readonly IMongoCollection<GeoPoint> _geoCollection;

    public MapService(
        IOptions<MapDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      _circleCollection = mongoDatabase.GetCollection<Marker>(
          bookStoreDatabaseSettings.Value.ObjectsCollectionName);

      _geoCollection = mongoDatabase.GetCollection<GeoPoint>(
          bookStoreDatabaseSettings.Value.GeoCollectionName);
    }

    public async Task<List<Marker>> GetAsync(List<string> ids)
    {
      var list = await _circleCollection.Find(i => ids.Contains(i.id)).ToListAsync();
      return list;
    }

    public async Task<Marker> GetAsync(string id) =>
        await _circleCollection.Find(x => x.id == id).FirstOrDefaultAsync();

    public async Task<List<Marker>> GetByParentIdAsync(string parent_id)
    {
      return await _circleCollection.Find(x => x.parent_id == parent_id).ToListAsync();
    }

    public async Task<List<Marker>> GetAllChildren(string parent_id)
    {
      List<Marker> result = new List<Marker>();
      ConcurrentBag<List<Marker>> cb = new ConcurrentBag<List<Marker>>();

      var children = await GetByParentIdAsync(parent_id);
      cb.Add(children);

      foreach (var item in children)
      {
        var sub_children = await GetAllChildren(item.id);
        cb.Add(sub_children);
      }

      foreach (var list in cb)
      {
        result.AddRange(list);
      }

      return result;
    }

    public async Task<List<Marker>> GetTopChildren(List<string> parentIds)
    {
      var result = await _circleCollection
        .Aggregate()
        .Match(x => parentIds.Contains(x.parent_id))
        //.Group("{ _id : '$parent_id'}")
        .Group(
          z => z.parent_id,
          g => new Marker() { id = g.Key })
        .ToListAsync();
      return result;
    }

    public async Task CreateAsync(Marker newBook) =>
        await _circleCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Marker updatedBook) =>
        await _circleCollection.ReplaceOneAsync(x => x.id == id, updatedBook);

    public async Task RemoveAsync(string id)
    {
      await _circleCollection.DeleteOneAsync(x => x.id == id);
      await _geoCollection.DeleteOneAsync(x => x.id == id);
    }
        

    public async Task<DeleteResult> RemoveAsync(List<string> ids)
    {
      //var idsFilter = Builders<Marker>.Filter.In(d => d.id, ids);
      //return await _circleCollection.DeleteManyAsync(idsFilter);
      await _geoCollection.DeleteManyAsync(x => ids.Contains(x.id));
      return await _circleCollection.DeleteManyAsync(x => ids.Contains(x.id));
    }

    public async Task CreateCompleteObject(FigureBaseDTO figure)
    {
      Marker marker = new Marker();
      marker.name = figure.name;
      marker.parent_id = figure.parent_id;

      await CreateAsync(marker);

      figure.id = marker.id;

      if (figure is FigureCircleDTO circle)
      {
        await CreateGeoPointAsync(circle);
      }
      
      if (figure is FigurePolygonDTO poligon)
      {
        await CreateGeoPoligonAsync(poligon);
      }
    }

    public async Task CreateGeoPointAsync(FigureCircleDTO newObject)
    {
      GeoPoint point = new GeoPoint();
      point.location = new GeoJsonPoint<GeoJson2DCoordinates>(
        GeoJson.Position(newObject.geometry[1], newObject.geometry[0])
      );
      point.id = newObject.id;
      await _geoCollection.InsertOneAsync(point);
    }

    public async Task CreateGeoPoligonAsync(FigurePolygonDTO newObject)
    {
      GeoPoint point = new GeoPoint();

      List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

      for(int i = 0; i <newObject.geometry.Length; i++)
      {
        coordinates.Add(GeoJson.Position(newObject.geometry[i][1], newObject.geometry[i][0]));
      }
      
      coordinates.Add(GeoJson.Position(newObject.geometry[0][1], newObject.geometry[0][0]));

      point.location = GeoJson.Polygon(coordinates.ToArray());

      point.id = newObject.id;
      await _geoCollection.InsertOneAsync(point);
    }

    private static void Log(FilterDefinition<GeoPoint> filter)
    {
      var serializerRegistry = BsonSerializer.SerializerRegistry;
      var documentSerializer = serializerRegistry.GetSerializer<GeoPoint>();
      var rendered = filter.Render(documentSerializer, serializerRegistry);
      Console.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
      Console.WriteLine();
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
  }
}
