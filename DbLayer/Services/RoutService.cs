using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DbLayer.Models.DBRoutLine;
using static Domain.StateWebSock.RoutLineDTO;

namespace DbLayer.Services
{
  public class RoutService : IRoutService
  {
    private readonly IMongoCollection<DBRoutLine> _collRouts;
    private readonly MongoClient _mongoClient;
    private readonly ILevelService _levelService;
    public RoutService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      ILevelService levelService
    )
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      var filter = new BsonDocument("name", geoStoreDatabaseSettings.Value.RoutsCollectionName);
      var options = new ListCollectionNamesOptions { Filter = filter };

      if (!mongoDatabase.ListCollectionNames(options).Any())
      {
        var createOptions = new CreateCollectionOptions();

        var timeField = nameof(DBRoutLine.ts_end);
        var metaField = nameof(DBRoutLine.meta);
        createOptions.TimeSeriesOptions =
          new TimeSeriesOptions(timeField, metaField, TimeSeriesGranularity.Seconds);


        mongoDatabase.CreateCollection(
          geoStoreDatabaseSettings.Value.RoutsCollectionName,
          createOptions);
      }

      _collRouts =
        mongoDatabase.GetCollection<DBRoutLine>(
          geoStoreDatabaseSettings.Value.RoutsCollectionName
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

        _collRouts.Indexes.CreateOneAsync(indexModel);
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

        _collRouts.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBRoutLine> keys =
              new IndexKeysDefinitionBuilder<DBRoutLine>()
              .Ascending(d => d.processed);

        var indexModel = new CreateIndexModel<DBRoutLine>(
          keys, new CreateIndexOptions()
          { Name = "processed" }
        );

        _collRouts.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task InsertManyAsync(List<RoutLineDTO> newObjs)
    {
      List<DBRoutLine> list = new List<DBRoutLine>();

      foreach (var track in newObjs)
      {
        var dbTrack = ConvertDTO2DB(track);
        list.Add(dbTrack);
      }
      await _collRouts.InsertManyAsync(list);
    }

    private DBRoutLine ConvertDTO2DB(RoutLineDTO track)
    {
      var dbTrack = new DBRoutLine()
      {        
        id_start = track.id_start,
        id_end = track.id_end,
        ts_start = track.ts_start,
        ts_end = track.ts_end,
        processed = (int)track.processed
      };

      dbTrack.meta.id = track.id;

      if (string.IsNullOrEmpty(dbTrack.meta.id))
      {
        dbTrack.meta.id = ObjectId.GenerateNewId().ToString();
      }

      dbTrack.meta.figure = ModelGate.ConvertDTO2DB(track.figure);
      return dbTrack;
    }

    public async Task DeleteManyAsync(List<string> ids)
    {
      var result = await _collRouts.DeleteManyAsync(x => ids.Contains(x.meta.id));
    }

    public async Task<List<RoutLineDTO>> GetAsync(int nLimit)
    {
      var dbTracks =
        await _collRouts.Find(t => t.meta.id != null).Limit(nLimit).ToListAsync();

      return ConvertListDB2DTO(dbTracks);
    }
    public async Task<List<RoutLineDTO>> GetNotProcessedAsync(int limit)
    {
      var dbTracks =
        await _collRouts.Find(t => t.processed == (int)RoutLineDTO.EntityType.not_processed)
        .Limit(limit)
        .ToListAsync();

      return ConvertListDB2DTO(dbTracks);
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
    public async Task<List<RoutLineDTO>> GetRoutsByBox(BoxTrackDTO box)
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
          || p.meta.figure.zoom_level == null);
      }


      filter = filter & builder.Where(t => t.ts_end >= box.time_start
        && t.ts_end <= box.time_end && t.processed == (int)EntityType.processsed);


      var dbObjects = new List<DBRoutLine>();

      if (box.ids != null &&
        (box.ids.Count > 0 || (box.property_filter != null && box.property_filter.props.Count > 0))
      )
      {
        filter = filter & builder.Where(t => box.ids.Contains(t.meta.figure.id));
      }

      var finder = _collRouts.Find(filter).Limit(limit);

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
