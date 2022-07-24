using DbLayer.Models;
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
using System.Threading.Tasks;

namespace DbLayer
{
  public class MapService
  {
    private readonly IMongoCollection<Marker> _markerCollection;

    private readonly IMongoCollection<MarkerProperties> _propCollection;

    private readonly MongoClient _mongoClient;

    public GeoService GeoServ 
    { 
      get; 
      private set; 
    }

    public TrackService TracksServ
    {
      get;
      private set;
    }

    public LevelService LevelServ
    {
      get;
      private set;
    }

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
          geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _markerCollection = mongoDatabase.GetCollection<Marker>(
          geoStoreDatabaseSettings.Value.ObjectsCollectionName);

      _propCollection = mongoDatabase.GetCollection<MarkerProperties>(
          geoStoreDatabaseSettings.Value.PropCollectionName);


      var geoCollection = mongoDatabase.GetCollection<GeoPoint>(
        geoStoreDatabaseSettings.Value.GeoCollectionName);

      var geoRawCollection =
        mongoDatabase.GetCollection<BsonDocument>(geoStoreDatabaseSettings.Value.GeoCollectionName);

      GeoServ = CreateGeoService(mongoDatabase, geoStoreDatabaseSettings.Value.GeoCollectionName);

      var tracksCollection =
        mongoDatabase.GetCollection<TrackPoint>(geoStoreDatabaseSettings.Value.TracksCollectionName);

      TracksServ = new TrackService(tracksCollection);

      var levelCollection =
        mongoDatabase.GetCollection<Level>(geoStoreDatabaseSettings.Value.LevelCollectionName);

      LevelServ = new LevelService(levelCollection);
    }

    private GeoService CreateGeoService(IMongoDatabase mongoDatabase, string collName)
    {
      var geoCollection = mongoDatabase.GetCollection<GeoPoint>(collName);

      var geoRawCollection =
        mongoDatabase.GetCollection<BsonDocument>(collName);

      return new GeoService(this, geoCollection, geoRawCollection);
    }

    public async Task<List<Marker>> GetAsync(List<string> ids)
    {
      var list = await _markerCollection.Find(i => ids.Contains(i.id)).ToListAsync();
      return list;
    }

    public async Task<Marker> GetAsync(string id) =>
        await _markerCollection.Find(x => x.id == id).FirstOrDefaultAsync();

    public async Task<List<Marker>> GetByParentIdAsync(string parent_id)
    {
      return await _markerCollection.Find(x => x.parent_id == parent_id).ToListAsync();
    }

    public async Task<List<Marker>> GetByChildIdAsync(string object_id)
    {
      List<Marker> parents = new List<Marker>();
      var marker = await GetAsync(object_id);

      while (marker != null)
      {
        parents.Add(marker);
        marker = await _markerCollection.Find(x => x.id == marker.parent_id).FirstOrDefaultAsync();
      }
      return parents;
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
      var result = await _markerCollection
        .Aggregate()
        .Match(x => parentIds.Contains(x.parent_id))
        //.Group("{ _id : '$parent_id'}")
        .Group(
          z => z.parent_id,
          g => new Marker() { id = g.Key })
        .ToListAsync();
      return result;
    }

    public async Task CreateAsync(Marker newObj)
    {
      await _markerCollection.InsertOneAsync(newObj);
    }

    public async Task UpdateAsync(Marker updatedObj)
    {
      using (var session = await _mongoClient.StartSessionAsync())
      {
        await UpdateAsync(session, updatedObj);
      }
    }

    private async Task UpdateAsync(IClientSessionHandle session, Marker updatedObj)
    {
      await _markerCollection.ReplaceOneAsync(session, x => x.id == updatedObj.id, updatedObj);
    } 

    public async Task<DeleteResult> RemoveAsync(List<string> ids)
    {
      await GeoServ.RemoveAsync(ids);
      await _propCollection.DeleteManyAsync(x => ids.Contains(x.id));
      return await _markerCollection.DeleteManyAsync(x => ids.Contains(x.id));
    }


    public async Task<GeoPoint> CreateCompleteObject(FigureBaseDTO figure)
    { 
      Marker marker = new Marker();
      marker.name = figure.name;
      marker.parent_id = figure.parent_id;

      if (!string.IsNullOrEmpty(figure.id))
      {
        marker.id = figure.id;
        await UpdateAsync(marker);
      }
      else
      {
        await CreateAsync(marker);
      }

      figure.id = marker.id;

      return await CreateGeoPoint(figure);
    }

    public async Task<GeoPoint> CreateGeoPoint(FigureBaseDTO figure)
    {
      GeoPoint geoPoint = null;

      if (figure is FigureCircleDTO circle)
      {
        geoPoint = await GeoServ.CreateOrUpdateGeoPointAsync(circle);
      }

      if (figure is FigurePolygonDTO polygon)
      {
        geoPoint = await GeoServ.CreateOrUpdateGeoPolygonAsync(polygon);
      }

      if (figure is FigurePolylineDTO polyline)
      {
        geoPoint = await GeoServ.CreateOrUpdateGeoPolylineAsync(polyline);
      }

      return geoPoint;
    }

    public async Task UpdatePropAsync(MarkerProperties updatedObj)
    {
      using (var session = await _mongoClient.StartSessionAsync())
      {
        await UpdatePropAsync(session, updatedObj);
      }
    }

    private async Task UpdatePropAsync(IClientSessionHandle session, MarkerProperties updatedObj)
    {
      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;
      await _propCollection.ReplaceOneAsync(session, x => x.id == updatedObj.id, updatedObj, opt);
    }

    public async Task<MarkerProperties> GetPropAsync(string id)
    {
      var obj = await _propCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return obj;
    }
  }
}
