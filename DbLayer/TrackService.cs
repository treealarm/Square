using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.StateWebSock;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public class TrackService
  {
    private readonly IMongoCollection<DBTrackPoint> _collection;
    public TrackService(IMongoCollection<DBTrackPoint> collection)
    {
      _collection = collection;
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
      await _collection.InsertManyAsync(list);
    }
  }
}
