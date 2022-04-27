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
    private readonly IMongoCollection<Marker> _markerCollection;
    private readonly IMongoCollection<GeoPoint> _geoCollection;
    private readonly MongoClient _mongoClient;

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
          geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _markerCollection = mongoDatabase.GetCollection<Marker>(
          geoStoreDatabaseSettings.Value.ObjectsCollectionName);

      _geoCollection = mongoDatabase.GetCollection<GeoPoint>(
          geoStoreDatabaseSettings.Value.GeoCollectionName);
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

    public async Task CreateAsync(IClientSessionHandle session, Marker newObj)
    {
      await _markerCollection.InsertOneAsync(session, newObj);
    }

    public async Task UpdateAsync(Marker updatedObj)
    {
      await _markerCollection.ReplaceOneAsync(x => x.id == updatedObj.id, updatedObj);
    }

    public async Task UpdateAsync(IClientSessionHandle session, Marker updatedObj)
    {
      await _markerCollection.ReplaceOneAsync(session, x => x.id == updatedObj.id, updatedObj);
    }        

    public async Task RemoveAsync(string id)
    {
      await _markerCollection.DeleteOneAsync(x => x.id == id);
      await _geoCollection.DeleteOneAsync(x => x.id == id);
    }
        

    public async Task<DeleteResult> RemoveAsync(List<string> ids)
    {
      //var idsFilter = Builders<Marker>.Filter.In(d => d.id, ids);
      //return await _circleCollection.DeleteManyAsync(idsFilter);
      await _geoCollection.DeleteManyAsync(x => ids.Contains(x.id));
      return await _markerCollection.DeleteManyAsync(x => ids.Contains(x.id));
    }


    private async Task DoCreateCompleteObject(FigureBaseDTO figure, IClientSessionHandle session)
    {
      Marker marker = new Marker();
      marker.name = figure.name;
      marker.parent_id = figure.parent_id;

      if (!string.IsNullOrEmpty(figure.id))
      {
        marker.id = figure.id;
        await UpdateAsync(session, marker);
      }
      else
      {
        await CreateAsync(session, marker);
      }


      figure.id = marker.id;

      if (figure is FigureCircleDTO circle)
      {
        await CreateOrUpdateGeoPointAsync(session, circle);
      }

      if (figure is FigurePolygonDTO polygon)
      {
        await CreateOrUpdateGeoPolygonAsync(session, polygon);
      }

      if (figure is FigurePolylineDTO polyline)
      {
        await CreateOrUpdateGeoPolylineAsync(session, polyline);
      }
    }
    public async Task CreateCompleteObject(FigureBaseDTO figure)
    {
      using (var session = await _mongoClient.StartSessionAsync())
      {
        //session.StartTransaction();
        try
        {
          await DoCreateCompleteObject(figure, session);

          //await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
          //await session.AbortTransactionAsync();
        }
      }
    }

    public async Task CreateOrUpdateGeoPointAsync(IClientSessionHandle session, FigureCircleDTO newObject)
    {
      GeoPoint point = new GeoPoint();
      point.location = new GeoJsonPoint<GeoJson2DCoordinates>(
        GeoJson.Position(newObject.geometry[1], newObject.geometry[0])
      );
      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(session, x => x.id == newObject.id, point);
      
      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(session, point);
      }
    }

    public async Task CreateOrUpdateGeoPolygonAsync(IClientSessionHandle session, FigurePolygonDTO newObject)
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
      var result = await _geoCollection.ReplaceOneAsync(session, x => x.id == newObject.id, point);

      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(session, point);
      }
    }

    public async Task CreateOrUpdateGeoPolylineAsync(IClientSessionHandle session, FigurePolylineDTO newObject)
    {
      GeoPoint point = new GeoPoint();

      List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

      for (int i = 0; i < newObject.geometry.Length; i++)
      {
        coordinates.Add(GeoJson.Position(newObject.geometry[i][1], newObject.geometry[i][0]));
      }

      point.location = GeoJson.LineString(coordinates.ToArray());

      point.id = newObject.id;
      var result = await _geoCollection.ReplaceOneAsync(session, x => x.id == newObject.id, point);
      if (result.MatchedCount <= 0)
      {
        await _geoCollection.InsertOneAsync(session, point);
      }
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
