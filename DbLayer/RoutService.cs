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
    private readonly IMongoCollection<DBTrackPoint> _collRouts;
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
        mongoDatabase.GetCollection<DBTrackPoint>(
          geoStoreDatabaseSettings.Value.RoutsCollectionName
        );

      _levelService = levelService;
    }

    public async Task InsertManyAsync(List<TrackPointDTO> newObjs)
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
      await _collRouts.InsertManyAsync(list);
    }

    public async Task<List<TrackPointDTO>> GetAsync()
    {
      List<TrackPointDTO> list = new List<TrackPointDTO>();
      var dbTracks =
        await _collRouts.Find(t => t.id != null).Limit(100).ToListAsync();

      return ConvertListDB2DTO(dbTracks);
    }

    private List<TrackPointDTO> ConvertListDB2DTO(List<DBTrackPoint> dbTracks)
    {
      var list = new List<TrackPointDTO>();

      foreach (var t in dbTracks)
      {
        var dto = new TrackPointDTO()
        {
          id = t.id,
          timestamp = t.timestamp,
          figure = ModelGate.ConvertDB2DTO(t.figure)
        };

        list.Add(dto);
      }

      return list;
    }

    public async Task<List<TrackPointDTO>> GetRoutsByBox(BoxDTO box)
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


      var list = await _collRouts.Find(filter).ToListAsync();

      return ConvertListDB2DTO(list);
    }
  }
}
