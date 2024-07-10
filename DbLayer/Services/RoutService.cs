using DbLayer.Models;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class RoutService : IRoutService
  {
    private readonly IMongoCollection<DBRoutLine> _collRoutes;
    private readonly IMongoClient _mongoClient;
    private readonly ILevelService _levelService;
    public RoutService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      ILevelService levelService,
      IMongoClient mongoClient
    )
    {
      _mongoClient = mongoClient;

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      var filter = new BsonDocument("name", geoStoreDatabaseSettings.Value.RoutesCollectionName);
      var options = new ListCollectionNamesOptions { Filter = filter };

      try
      {
        if (!mongoDatabase.ListCollectionNames(options).Any())
        {
          var createOptions = new CreateCollectionOptions();

          var timeField = nameof(DBRoutLine.ts_end);
          var metaField = nameof(DBRoutLine.meta);
          createOptions.TimeSeriesOptions =
            new TimeSeriesOptions(timeField, metaField, TimeSeriesGranularity.Seconds);


          mongoDatabase.CreateCollection(
            geoStoreDatabaseSettings.Value.RoutesCollectionName,
            createOptions);
        }
      }
      catch ( Exception ex )
      {
        Console.WriteLine( ex.ToString() );
      }
      

      _collRoutes =
        mongoDatabase.GetCollection<DBRoutLine>(
          geoStoreDatabaseSettings.Value.RoutesCollectionName
        );

      _levelService = levelService;

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBRoutLine> keys =
              new IndexKeysDefinitionBuilder<DBRoutLine>()
              .Geo2DSphere(d => d.meta.figure.location)
              ;
        //"{ 'figure.location': '2dsphere', ts_start: 1, ts_end: 1 }";
        var indexModel = new CreateIndexModel<DBRoutLine>(
          keys, new CreateIndexOptions()
          { Name = "location" }
        );

        _collRoutes.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBRoutLine> keys =
              new IndexKeysDefinitionBuilder<DBRoutLine>()
              .Ascending(d => d.meta.figure.zoom_level)
              .Ascending(d => d.ts_end);

        var indexModel = new CreateIndexModel<DBRoutLine>(
          keys, new CreateIndexOptions()
          { Name = "compound" }
        );

        _collRoutes.Indexes.CreateOneAsync(indexModel);
      }

      {
        // Index for faste del.
        IndexKeysDefinition<DBRoutLine> keys =
              new IndexKeysDefinitionBuilder<DBRoutLine>()
              .Ascending(d => d.meta.id);

        var indexModel = new CreateIndexModel<DBRoutLine>(
          keys, new CreateIndexOptions()
          { Name = "mid" }
        );

        _collRoutes.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task<long> InsertManyAsync(List<RoutLineDTO> newObjs)
    {
      List<DBRoutLine> list = new List<DBRoutLine>();

      foreach (var track in newObjs)
      {
        var dbTrack = ConvertDTO2DB(track);

        if (dbTrack != null)
        {
          list.Add( dbTrack );
        }
      }
      await _collRoutes.InsertManyAsync(list);
      return (long)newObjs.Count;
    }

    private DBRoutLine ConvertDTO2DB(RoutLineDTO track)
    {
      var dbTrack = new DBRoutLine()
      {        
        id_start = track.id_start,
        id_end = track.id_end,
        ts_start = track.ts_start,
        ts_end = track.ts_end,
      };

      dbTrack.meta.id = track.id;

      if (string.IsNullOrEmpty(dbTrack.meta.id))
      {
        dbTrack.meta.id = ObjectId.GenerateNewId().ToString();
      }

      if (track.figure == null)
      {
        return null;
      }
      dbTrack.meta.figure = ModelGate.ConvertDTO2DB(track.figure);
      return dbTrack;
    }

    public async Task DeleteManyAsync(List<string> ids)
    {
      var result = await _collRoutes.DeleteManyAsync(x => ids.Contains(x.meta.id));
    }

    public async Task<List<RoutLineDTO>> GetByIdsAsync(List<string> ids)
    {
      var dbTracks =
        await _collRoutes
        .Find(t => ids.Contains(t.id_start) || ids.Contains(t.id_end))
        .ToListAsync();

      return ConvertListDB2DTO(dbTracks);
    }

    public async Task<List<string>> GetProcessedIdsAsync(List<string> ids)
    {
      var dbTracks =
        await _collRoutes
        .AsQueryable<DBRoutLine>()
        .Where<DBRoutLine>(t => ids.Contains(t.id_end))
        
        .Select(t => t.id_end)
        .ToListAsync()
        ;
      return dbTracks;
    }

    private List<RoutLineDTO> ConvertListDB2DTO(List<DBRoutLine> dbTracks)
    {
      var list = new List<RoutLineDTO>();

      foreach (var t in dbTracks)
      {
        var dto = new RoutLineDTO()
        {
          id = t.meta.id,
          id_start = t.id_start,
          id_end = t.id_end,
          figure = ModelGate.ConvertDB2DTO(t.meta.figure),
          ts_start = t.ts_start,
          ts_end = t.ts_end,
        };

        list.Add(dto);
      }

      return list;
    }
    public async Task<List<RoutLineDTO>> GetRoutesByBox(BoxTrackDTO box)
    {
      int limit = 10000;

      if (box.count != null && box.count > 0)
      {
        limit = box.count.Value;
      }

      var builder = Builders<DBRoutLine>.Filter;
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

      FilterDefinition<DBRoutLine> filter =
        builder.GeoIntersects(t => t.meta.figure.location, geometry);

      if (box.zoom != null)
      {
        var levels = await _levelService.GetLevelsByZoom(box.zoom);

        filter = filter &
          builder.Where(p => levels.Contains(p.meta.figure.zoom_level)
          || string.IsNullOrEmpty(p.meta.figure.zoom_level));
      }


      filter = filter & builder.Where(t => t.ts_end >= box.time_start
        && t.ts_end <= box.time_end);


      var dbObjects = new List<DBRoutLine>();

      if (box.ids != null &&
        (box.ids.Count > 0 || (box.property_filter != null && box.property_filter.props.Count > 0))
      )
      {
        filter = filter & builder.Where(t => box.ids.Contains(t.meta.figure.id));
      }

      var finder = _collRoutes.Find(filter).Limit(limit);

      if (box.sort < 0)
      {
        // Desc is bad on time series.
        //finder = finder.SortByDescending(el => el.timestamp);
      }


      var list = await finder.ToListAsync();

      dbObjects.AddRange(list);


      return ConvertListDB2DTO(dbObjects);
    }
  }
}
