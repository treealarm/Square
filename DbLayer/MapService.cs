using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
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
    private readonly IMongoCollection<DBMarker> _markerCollection;

    private readonly IMongoCollection<DBMarkerProperties> _propCollection;

    private readonly MongoClient _mongoClient;

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
          geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _markerCollection = mongoDatabase.GetCollection<DBMarker>(
          geoStoreDatabaseSettings.Value.ObjectsCollectionName);

      _propCollection = mongoDatabase.GetCollection<DBMarkerProperties>(
          geoStoreDatabaseSettings.Value.PropCollectionName);

    }

    public async Task<List<DBMarker>> GetAsync(List<string> ids)
    {
      var list = await _markerCollection.Find(i => ids.Contains(i.id)).ToListAsync();
      return list;
    }

    public async Task<DBMarker> GetAsync(string id) =>
        await _markerCollection.Find(x => x.id == id).FirstOrDefaultAsync();

    public async Task<List<DBMarker>> GetByParentIdAsync(string parent_id)
    {
      return await _markerCollection.Find(x => x.parent_id == parent_id).ToListAsync();
    }

    public async Task<List<DBMarker>> GetByChildIdAsync(string object_id)
    {
      List<DBMarker> parents = new List<DBMarker>();
      var marker = await GetAsync(object_id);

      while (marker != null)
      {
        parents.Add(marker);
        marker = await _markerCollection.Find(x => x.id == marker.parent_id).FirstOrDefaultAsync();
      }
      return parents;
    }

    public async Task<List<DBMarker>> GetAllChildren(string parent_id)
    {
      List<DBMarker> result = new List<DBMarker>();
      ConcurrentBag<List<DBMarker>> cb = new ConcurrentBag<List<DBMarker>>();

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

    public async Task<List<DBMarker>> GetTopChildren(List<string> parentIds)
    {
      var result = await _markerCollection
        .Aggregate()
        .Match(x => parentIds.Contains(x.parent_id))
        //.Group("{ _id : '$parent_id'}")
        .Group(
          z => z.parent_id,
          g => new DBMarker() { id = g.Key })
        .ToListAsync();
      return result;
    }

    public async Task CreateAsync(DBMarker newObj)
    {
      await _markerCollection.InsertOneAsync(newObj);
    }

    public async Task UpdateAsync(DBMarker updatedObj)
    {
      using (var session = await _mongoClient.StartSessionAsync())
      {
        await UpdateAsync(session, updatedObj);
      }
    }

    private async Task UpdateAsync(IClientSessionHandle session, DBMarker updatedObj)
    {
      await _markerCollection.ReplaceOneAsync(session, x => x.id == updatedObj.id, updatedObj);
    } 

    public async Task<DeleteResult> RemoveAsync(List<string> ids)
    {
      
      await _propCollection.DeleteManyAsync(x => ids.Contains(x.id));
      return await _markerCollection.DeleteManyAsync(x => ids.Contains(x.id));
    }

    public async Task<FigureBaseDTO> CreateCompleteObject(FigureBaseDTO figure)
    { 
      DBMarker marker = new DBMarker();
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

      return figure;
    }

    public async Task UpdatePropAsync(DBMarkerProperties updatedObj)
    {
      using (var session = await _mongoClient.StartSessionAsync())
      {
        await UpdatePropAsync(session, updatedObj);
      }
    }

    private async Task UpdatePropAsync(IClientSessionHandle session, DBMarkerProperties updatedObj)
    {
      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;
      await _propCollection.ReplaceOneAsync(session, x => x.id == updatedObj.id, updatedObj, opt);
    }

    public async Task<DBMarkerProperties> GetPropAsync(string id)
    {
      var obj = await _propCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return obj;
    }
  }
}
