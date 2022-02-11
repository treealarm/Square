using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public class MapService
  {
    private readonly IMongoCollection<CircleMarker> _circleCollection;

    public MapService(
        IOptions<MapDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      _circleCollection = mongoDatabase.GetCollection<CircleMarker>(
          bookStoreDatabaseSettings.Value.ObjectsCollectionName);
    }

    public async Task<List<CircleMarker>> GetAsync() =>
        await _circleCollection.Find(_ => true).ToListAsync();

#nullable enable
    public async Task<CircleMarker?> GetAsync(string id) =>
        await _circleCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
#nullable disable

    public async Task CreateAsync(CircleMarker newBook) =>
        await _circleCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, CircleMarker updatedBook) =>
        await _circleCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _circleCollection.DeleteOneAsync(x => x.Id == id);
  }
}
