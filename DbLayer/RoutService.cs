﻿using DbLayer.Models;
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
          figure = ModelGate.ConvertDTO2DB(track.figure)
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
          figure = ModelGate.ConvertDB2DTO(t.figure)
        };

        list.Add(dto);
      }

      return list;
    }

    public async Task<List<RoutLineDTO>> GetRoutsByBox(BoxDTO box)
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

      var levels = await _levelService.GetLevelsByZoom(box.zoom);

      var filter =
          builder.Where(p => levels.Contains(p.figure.zoom_level))
        & builder.GeoIntersects(t => t.figure.location, geometry);


      var list = await _collRouts.Find(filter).ToListAsync();

      return ConvertListDB2DTO(list);
    }
  }
}