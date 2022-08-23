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
    public TrackService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _collFigures =
        mongoDatabase.GetCollection<DBTrackPoint>(
          geoStoreDatabaseSettings.Value.TracksCollectionName
        );
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
    public async Task<List<TrackPointDTO>> GetAsync()
    { 
      var dbTracks = 
        await _collFigures.Find(t => t.id != null).Limit(100).ToListAsync();

      return DBListToDTO(dbTracks);
    }
  }
}
