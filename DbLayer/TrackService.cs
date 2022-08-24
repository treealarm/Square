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
  public class TrackService: ITrackService
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
      await _collFigures.InsertManyAsync(list);

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

    public async Task<TrackPointDTO> GetLastAsync(string figure_id, string ignoreTrackId)
    {
      var dbTrack =
        await _collFigures
          .Find(t => t.figure.id == figure_id && t.id != ignoreTrackId)
          .SortByDescending(t => t.id)
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
    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxDTO box)
    {
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

      var levels = await _levelService.GetLevelsByZoom(box.zoom);

      var filter =
          builder.Where(p => levels.Contains(p.figure.zoom_level))
        & builder.GeoIntersects(t => t.figure.location, geometry);


      var dbTracks = await _collFigures.Find(filter).ToListAsync();


      return DBListToDTO(dbTracks);
    }
  }
}
