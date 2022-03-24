using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

    public async Task CreateGeoAsync(GeometryDTO newObject)
    {
      GeoPoint point = new GeoPoint();
      point.coordinates = new GeoJsonPoint<GeoJson2DCoordinates>(
        GeoJson.Position(newObject.coordinates[0], newObject.coordinates[1])
      );
      point.id = newObject.id;
      await _geoCollection.InsertOneAsync(point);
    }

    public async Task<List<GeoPoint>> GetGeoAsync()
    {
      var list = await _geoCollection.Find(_ => true).ToListAsync();
      return list;
    }
  }
}
