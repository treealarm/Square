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
    public RoutService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _collRouts =
        mongoDatabase.GetCollection<DBTrackPoint>(
          geoStoreDatabaseSettings.Value.RoutsCollectionName
        );
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
  }
}
