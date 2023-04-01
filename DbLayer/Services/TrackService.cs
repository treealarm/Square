using Domain.GeoDTO;
using Domain.OptionsModels;
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

      

      var filter = new BsonDocument("name", geoStoreDatabaseSettings.Value.TracksCollectionName);
      var options = new ListCollectionNamesOptions { Filter = filter };

      try
      {
        if (!mongoDatabase.ListCollectionNames(options).Any())
        {
          var createOptions = new CreateCollectionOptions();

          var timeField = nameof(DBTrackPoint.timestamp);
          var metaField = nameof(DBTrackPoint.meta);

          createOptions.TimeSeriesOptions =
            new TimeSeriesOptions(timeField, metaField, TimeSeriesGranularity.Seconds);

          mongoDatabase.CreateCollection(
          geoStoreDatabaseSettings.Value.TracksCollectionName,
          createOptions);
        }
      }
      catch ( Exception ex )
      {
        Console.WriteLine( ex.ToString() );
      }

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
                .Geo2DSphere(d => d.meta.figure.location)
                .Ascending(d => d.meta.figure.zoom_level)
                .Ascending(d => d.meta.figure.id)
                .Ascending(d => d.timestamp)
                ;

        var indexModel = new CreateIndexModel<DBTrackPoint>(
          keys, new CreateIndexOptions()
          { Name = "combo" }
        );

        _collFigures.Indexes.CreateOneAsync(indexModel);
      }
      {
        IndexKeysDefinition<DBTrackPoint> keys =
                new IndexKeysDefinitionBuilder<DBTrackPoint>()
                  .Ascending(d => d.meta.id)
                ;

        var indexModel = new CreateIndexModel<DBTrackPoint>(
          keys, new CreateIndexOptions()
          { Name = "mid" }
        );

        _collFigures.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBTrackPoint> keys =
                new IndexKeysDefinitionBuilder<DBTrackPoint>()
                  .Ascending(d => d.meta.figure.id)
                ;

        var indexModel = new CreateIndexModel<DBTrackPoint>(
          keys, new CreateIndexOptions()
          { Name = "fid" }
        );

        _collFigures.Indexes.CreateOneAsync(indexModel);
      }
      {
        IndexKeysDefinition<DBTrackPoint> keys =
                new IndexKeysDefinitionBuilder<DBTrackPoint>()
                  .Ascending(d => d.meta.figure.id)
                  .Ascending(d => d.timestamp)
                ;

        var indexModel = new CreateIndexModel<DBTrackPoint>(
          keys, new CreateIndexOptions()
          { Name = "fid_ts" }
        );

        _collFigures.Indexes.CreateOneAsync(indexModel);
      }

      {
        var keys = Builders<DBTrackPoint>.IndexKeys.Combine(
          Builders<DBTrackPoint>.IndexKeys
          .Ascending($"{nameof(DBTrackPoint.meta.extra_props)}.{nameof(DBObjExtraProperty.prop_name)}"),
          Builders<DBTrackPoint>.IndexKeys
          .Ascending($"{nameof(DBTrackPoint.meta.extra_props)}.{nameof(DBObjExtraProperty.str_val)}"));

        var indexModel = new CreateIndexModel<DBTrackPoint>(
           keys, new CreateIndexOptions()
           { Name = "ep" }
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
          timestamp = track.timestamp          
        };
        dbTrack.meta.id = ObjectId.GenerateNewId().ToString();
        dbTrack.meta.figure = ModelGate.ConvertDTO2DB(track.figure);
        dbTrack.meta.extra_props = ModelGate.ConvertExtraPropsToDB(track.extra_props);
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
        id = t.meta.id,
        timestamp = t.timestamp,
        figure = ModelGate.ConvertDB2DTO(t.meta.figure),
        extra_props = ModelGate.ConverDBExtraProp2DTO(t.meta.extra_props)
      };

      return dto;
    }

    public async Task<TrackPointDTO> GetLastAsync(string figure_id, DateTime beforeTime)
    {
      var builder = Builders<DBTrackPoint>.Filter;

      FilterDefinition<DBTrackPoint> filter =
        builder
        .Where(t => t.meta.figure.id == figure_id && t.timestamp < beforeTime);

      Log(filter);

      var dbTrack =
        await _collFigures
          .Find(filter)
          .SortByDescending(t => t.timestamp)
          .FirstOrDefaultAsync();

      return ConvertDB2DTO(dbTrack);
    }

    public async Task<TrackPointDTO> GetByIdAsync(string id)
    {
      var dbTrack =
        await _collFigures.Find(t => t.meta.id == id)
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

    private static void Log<T>(FilterDefinition<T> filter)
    {
      var serializerRegistry = BsonSerializer.SerializerRegistry;
      var documentSerializer = serializerRegistry.GetSerializer<T>();
      var rendered = filter.Render(documentSerializer, serializerRegistry);
      Debug.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
      Debug.WriteLine("");
    }

    public async Task<List<TrackPointDTO>> GetFirstTracksByTime(
      DateTime? time_start,
      DateTime? time_end,
      List<string> figIds
    )
    {
      var idsFilled = (figIds != null && figIds.Count > 0);
      var ids = figIds;

      List<DBTrackPoint> dbTracks = new List<DBTrackPoint>();

      var builder = Builders<DBTrackPoint>.Filter;

      FilterDefinition<DBTrackPoint>  filter = FilterDefinition<DBTrackPoint>.Empty;

      if (time_start != null && time_end != null)
      {
        filter =
          builder.Where(t => t.timestamp >= time_start && t.timestamp <= time_end);      
      }

      if (ids != null && ids.Count > 0)
      {
        FilterDefinition<DBTrackPoint> f;

        foreach (var id in ids)
        {
          if (FilterDefinition<DBTrackPoint>.Empty == filter)
          {
            f = builder.Where(t => ids.Contains(t.meta.figure.id));
          }
          else
          {
            f = filter & builder.Where(t => t.meta.figure.id == id);
          }

          var track = await _collFigures.Find(f).FirstOrDefaultAsync();

          if (track != null)
          {
            dbTracks.Add(track);
          }
        }        
      }
      else
      {
        dbTracks = await _collFigures
          .Find(filter)
          .Limit(10000)
          .ToListAsync()
          ;

        dbTracks = dbTracks.DistinctBy(d => d.meta.id).ToList();
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

      GeoJsonGeometry<GeoJson2DCoordinates> geometry;
      FilterDefinition<DBTrackPoint> filter = null;

      if (box.zone != null)
      {
        foreach ( var zone in box.zone)
        {
          geometry = ModelGate.ConvertGeoDTO2DB(zone);
          var f1 = builder.GeoIntersects(t => t.meta.figure.location, geometry);

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

        filter = builder.GeoIntersects(t => t.meta.figure.location, geometry);
      }

      if (box.not_in_zone)
      {
        filter = builder.Not(filter);
      }

      if (box.zoom != null)
      {
        var levels = await _levelService.GetLevelsByZoom(box.zoom);

        filter = filter &
          builder.Where(p => levels.Contains(p.meta.figure.zoom_level)
          || string.IsNullOrEmpty(p.meta.figure.zoom_level));
      } 


      filter = filter & builder.Where(t => t.timestamp >= box.time_start
        && t.timestamp <= box.time_end);


      List<DBTrackPoint> dbObjects = new List<DBTrackPoint>();

      if (box.ids != null &&
        box.ids.Count > 0
      )
      {
        filter = filter & builder.Where(t => box.ids.Contains(t.meta.figure.id));
      }

      if (box.property_filter != null && box.property_filter.props.Count > 0)
      {
        foreach (var prop in box.property_filter.props)
        {
          var request =
            string.Format("{{prop_name:'{0}', str_val:'{1}'}}",
            prop.prop_name,
            prop.str_val);

          var f1 = Builders<DBTrackPoint>
            .Filter
            .ElemMatch(t => t.meta.extra_props, request)
            ;

          var metaValue = new BsonDocument(
              "str_val",
              prop.str_val
              );

          if (filter == builder.Empty)
          {
            filter = f1;
          }
          else
          {
            filter &= f1;
          }
        }
      }

      var finder = _collFigures.Find(filter).Limit(limit);

      if (box.sort < 0)
      {
        // Desc is bad on time series.
        //finder = finder.SortByDescending(el => el.timestamp);
      }

      Log(filter);

      var list = await finder
        .ToListAsync();

      dbObjects.AddRange(list);

      return DBListToDTO(dbObjects);
    }
  }
}
