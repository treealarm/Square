using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
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
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class TrackService : ITrackService
  {
    private readonly IMongoCollection<DBTrackPoint> _collFigures;
    private readonly MongoClient _mongoClient;
    private readonly ILevelService _levelService;
    public TrackService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      ILevelService levelService
    )
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _collFigures =
        mongoDatabase.GetCollection<DBTrackPoint>(
          geoStoreDatabaseSettings.Value.TracksCollectionName
        );
      _levelService = levelService;

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBTrackPoint> keys =
                new IndexKeysDefinitionBuilder<DBTrackPoint>()
                .Ascending(d => d.figure.zoom_level)
                .Geo2DSphere(d => d.figure.location)
                .Ascending(d => d.timestamp)
                .Ascending(d => d.figure.id)
                ;

        var indexModel = new CreateIndexModel<DBTrackPoint>(
          keys, new CreateIndexOptions()
          { Name = "location" }
        );

        _collFigures.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBTrackPoint> keys =
        new IndexKeysDefinitionBuilder<DBTrackPoint>()
        .Descending(d => d.timestamp)
        ;

        var indexModel = new CreateIndexModel<DBTrackPoint>(
          keys, new CreateIndexOptions()
          { Name = "timestamp" }
        );

        _collFigures.Indexes.CreateOneAsync(indexModel);
      }      
    }
    public async Task<List<TrackPointDTO>> InsertManyAsync(List<TrackPointDTO> newObjs)
    {
      List<DBTrackPoint> list = new List<DBTrackPoint>();

      foreach (var track in newObjs)
      {
        var dbTrack = new DBTrackPoint()
        {
          timestamp = track.timestamp,
          figure = ModelGate.ConvertDTO2DB(track.figure)
        };
        list.Add(dbTrack);
      }

      if (list.Count > 0)
      {
        await _collFigures.InsertManyAsync(list);
      }

      return DBListToDTO(list);
    }

    private TrackPointDTO ConvertDB2DTO(DBTrackPoint t)
    {
      if (t == null)
      {
        return null;
      }

      var dto = new TrackPointDTO()
      {
        id = t.id,
        timestamp = t.timestamp,
        figure = ModelGate.ConvertDB2DTO(t.figure)
      };

      return dto;
    }

    public async Task<TrackPointDTO> GetLastAsync(string figure_id, DateTime beforeTime)
    {
      var dbTrack =
        await _collFigures
          .Find(t => t.figure.id == figure_id && t.timestamp < beforeTime)
          .SortByDescending(t => t.timestamp)
          .FirstOrDefaultAsync();

      return ConvertDB2DTO(dbTrack);
    }

    public async Task<TrackPointDTO> GetByIdAsync(string id)
    {
      var dbTrack =
        await _collFigures.Find(t => t.id == id)
          .FirstOrDefaultAsync();

      return ConvertDB2DTO(dbTrack);
    }

    private List<TrackPointDTO> DBListToDTO(List<DBTrackPoint> dbTracks)
    {
      List<TrackPointDTO> list = new List<TrackPointDTO>();
      foreach (var t in dbTracks)
      {
        var dto = ConvertDB2DTO(t);
        list.Add(dto);
      }
      return list;
    }

    private static void Log(FilterDefinition<BsonDocument> filter)
    {
      var serializerRegistry = BsonSerializer.SerializerRegistry;
      var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
      var rendered = filter.Render(documentSerializer, serializerRegistry);
      Debug.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
      Debug.WriteLine("");
    }

    public async Task<List<TrackPointDTO>> GetTracksByTime(
      DateTime? time_start,
      DateTime? time_end,
      List<string> ids
    )
    {
      List<DBTrackPoint> dbTracks = new List<DBTrackPoint>();

      var builder = Builders<DBTrackPoint>.Filter;

      FilterDefinition<DBTrackPoint> filter = FilterDefinition<DBTrackPoint>.Empty;

      if (ids != null && ids.Count > 0 && time_start != null && time_end == null)
      {
        foreach (var id in ids)
        {
          var dbTracksTemp = await _collFigures
            .Find(t => t.figure.id == id && t.timestamp >= time_start)
            .FirstOrDefaultAsync();

          if ( dbTracksTemp!= null)
          {
            dbTracks.Add(dbTracksTemp);
          }
        }
      }
      else if (ids != null && ids.Count > 0 && time_start == null && time_end != null)
      {
        foreach (var id in ids)
        {
          var dbTracksTemp = await _collFigures
            .Find(t => t.figure.id == id && t.timestamp <= time_end)
            .SortByDescending(t => t.timestamp)
            .FirstOrDefaultAsync();

          if (dbTracksTemp != null)
          {
            dbTracks.Add(dbTracksTemp);
          }
        }
      }
      else
      {
        if (time_start != null)
        {
          filter = filter & builder.Where(t => t.timestamp >= time_start);
        }

        if (time_end != null)
        {
          filter = filter & builder.Where(t => t.timestamp <= time_end);
        }

        if (ids != null && ids.Count > 0)
        {
          filter = filter & builder.Where(t => ids.Contains(t.figure.id));
        }

        dbTracks.AddRange(await _collFigures.Find(filter).ToListAsync());
      }

      return DBListToDTO(dbTracks);
    }
    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
    {
      int limit = 10000;

      if (box.count != null && box.count > 0)
      {
        limit = box.count.Value;
      }

      var builder = Builders<DBTrackPoint>.Filter;
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

      FilterDefinition<DBTrackPoint> filter =
        builder.GeoIntersects(t => t.figure.location, geometry);

      if (box.zoom != null)
      {
        var levels = await _levelService.GetLevelsByZoom(box.zoom);

        filter = filter &
          builder.Where(p => levels.Contains(p.figure.zoom_level)
          || p.figure.zoom_level == null);
      }

      bool distinct = (box.time_start == null && box.time_end != null) 
        || (box.time_start != null && box.time_end == null);


      if (box.time_start != null)
      {
        filter = filter & builder.Where(t => t.timestamp >= box.time_start);
      }

      if (box.time_end != null)
      {
        filter = filter & builder.Where(t => t.timestamp <= box.time_end);
      }


      List<DBTrackPoint> dbTracks = new List<DBTrackPoint>();

      if (distinct)
      {
        var ids = await _collFigures
          .Distinct(el => el.figure.id, filter).ToListAsync();

        foreach (var id in ids)
        {
          var f1 = builder.Where(o => o.figure.id == id) & filter;
          DBTrackPoint el = null;

          if (box.time_end != null)
          {
            el = await _collFigures
            .Find(f1)
            .SortByDescending(o => o.id)
            .FirstOrDefaultAsync();
          }
          else
          {
            el = await _collFigures
            .Find(f1)
            .FirstOrDefaultAsync();
          }
            

          if (el != null)
          {
            dbTracks.Add(el);
          }
        }        
      }
      else
      {
        if (box.ids != null)
        {
          filter = filter & builder.Where(t => box.ids.Contains(t.figure.id));
        }

        dbTracks.AddRange(
          await _collFigures
          .Find(filter)
          .Limit(limit)
          .ToListAsync()
          );
      }
      return DBListToDTO(dbTracks);
    }
  }
}
