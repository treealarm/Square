using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
    private readonly IMongoCollection<Marker> _circleCollection;

    public MapService(
        IOptions<MapDatabaseSettings> bookStoreDatabaseSettings)
    {
      var mongoClient = new MongoClient(
          bookStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = mongoClient.GetDatabase(
          bookStoreDatabaseSettings.Value.DatabaseName);

      _circleCollection = mongoDatabase.GetCollection<Marker>(
          bookStoreDatabaseSettings.Value.ObjectsCollectionName);
    }

    public async Task<List<Marker>> GetAsync()
    {
      var list = await _circleCollection.Find(_ => true).ToListAsync();
      return list;
    }

    public async Task<Marker> GetAsync(string id) =>
        await _circleCollection.Find(x => x.id == id).FirstOrDefaultAsync();

    public async Task<List<Marker>> GetByParentIdAsync(string parent_id)
    {
      return await _circleCollection.Find(x => x.parent_id == parent_id).ToListAsync();
    }

    public async Task<List<Marker>> GetTopChildren(List<string> parentIds)
    {
      var result = await _circleCollection
        .Aggregate()
        .Match(x => parentIds.Contains(x.parent_id))
        //.Group("{ _id : '$parent_id'}")
        .Group(
          z => z.parent_id, 
          g => new Marker() { id = g.Key })
        .ToListAsync();
      return result;
    }

    public async Task CreateAsync(Marker newBook) =>
        await _circleCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Marker updatedBook) =>
        await _circleCollection.ReplaceOneAsync(x => x.id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _circleCollection.DeleteOneAsync(x => x.id == id);
  }
}
