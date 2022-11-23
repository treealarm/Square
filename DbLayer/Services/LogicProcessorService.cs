using DbLayer.Models;
using Domain;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class LogicProcessorService: ILogicProcessorService
  {
    private readonly IMongoCollection<DBLogicProcessor> _collection;
    private readonly MongoClient _mongoClient;
    public LogicProcessorService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _collection =
        mongoDatabase.GetCollection<DBLogicProcessor>(
          geoStoreDatabaseSettings.Value.LevelCollectionName
          );

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBLogicProcessor> keys =
          new IndexKeysDefinitionBuilder<DBLogicProcessor>()
          .Geo2DSphere(d => d.figure.location)
          ;

        var indexModel = new CreateIndexModel<DBLogicProcessor>(
          keys, new CreateIndexOptions()
          { Name = "geo" }
        );

        _collection.Indexes.CreateOneAsync(indexModel);
      }
    }
  }
}
