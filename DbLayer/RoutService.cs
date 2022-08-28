using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
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

      _collRouts =
        mongoDatabase.GetCollection<DBRoutLine>(
          geoStoreDatabaseSettings.Value.RoutsCollectionName
        );

      _levelService = levelService;

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      IndexKeysDefinition<DBRoutLine> keys =
      new IndexKeysDefinitionBuilder<DBRoutLine>()
      .Geo2DSphere(d => d.figure.location)
      .Ascending(d => d.figure.zoom_level)
      .Ascending(d => d.ts_start)
      .Ascending(d => d.ts_end);
      //"{ 'figure.location': '2dsphere', ts_start: 1, ts_end: 1 }";
      var indexModel = new CreateIndexModel<DBRoutLine>(
        keys, new CreateIndexOptions()
        { Name = "location" }
      );

      _collRouts.Indexes.CreateOneAsync(indexModel);
    }

    public async Task InsertManyAsync(List<RoutLineDTO> newObjs)
    {
      List<DBRoutLine> list = new List<DBRoutLine>();

      foreach (var track in newObjs)
      {
        var dbTrack = new DBRoutLine()
        {
          id_start = track.id_start,
          id_end = track.id_end,
          figure = ModelGate.ConvertDTO2DB(track.figure),
          ts_start = track.ts_start,
          ts_end = track.ts_end,
        };

        list.Add(dbTrack);
      }
      await _collRouts.InsertManyAsync(list);
    }

    public async Task<List<RoutLineDTO>> GetAsync()
    {
      var dbTracks =
        await _collRouts.Find(t => t.id != null).Limit(100).ToListAsync();

      return ConvertListDB2DTO(dbTracks);
    }

    private List<RoutLineDTO> ConvertListDB2DTO(List<DBRoutLine> dbTracks)
    {
      var list = new List<RoutLineDTO>();

      foreach (var t in dbTracks)
      {
        var dto = new RoutLineDTO()
        {
          id = t.id,
          id_start = t.id_start,
          id_end = t.id_end,
          figure = ModelGate.ConvertDB2DTO(t.figure),
          ts_start = t.ts_start,
          ts_end = t.ts_end,
        };

        list.Add(dto);
      }

      return list;
    }

    public async Task<List<RoutLineDTO>> GetRoutsByBox(BoxTrackDTO box)
    {
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

      FilterDefinition<DBRoutLine> filter = FilterDefinition<DBRoutLine>.Empty;

      if (box.zoom != null)
      {
        var levels = await _levelService.GetLevelsByZoom(box.zoom);

        filter =
          builder.Where(p => levels.Contains(p.figure.zoom_level)
          || p.figure.zoom_level == null);
      }

      if (box.time_start != null)
      {
        filter = filter & builder
          .Where(t => t.ts_start >= box.time_start || t.ts_start == null);
      }

      if (box.time_end != null)
      {
        filter = filter & builder
          .Where(t => t.ts_end <= box.time_end || t.ts_end == null);
      }

      filter = filter & builder.GeoIntersects(t => t.figure.location, geometry);


      var list = await _collRouts.Find(filter).ToListAsync();

      return ConvertListDB2DTO(list);
    }
  }
}
