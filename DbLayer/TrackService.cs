using MongoDB.Driver;
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

    public async Task InsertManyAsync(List<DBTrackPoint> newObjs)
    {
      await _collection.InsertManyAsync(newObjs);
    }
  }
}
