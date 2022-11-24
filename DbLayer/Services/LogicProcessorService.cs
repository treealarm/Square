using DbLayer.Models;
using Domain;
using Domain.GeoDTO;
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
    private IMongoCollection<DBLogicProcessor> _collection;
    private readonly MongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDatabase;
    private string _tableName;
    public LogicProcessorService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      _mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _tableName = geoStoreDatabaseSettings.Value.LogicProcessorCollectionName;
      _collection =
        _mongoDatabase.GetCollection<DBLogicProcessor>(
          _tableName
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

    public async Task<List<string>> GetLogicByFigure(GeoObjectDTO figure)
    {
      var location = ModelGate.ConvertGeoDTO2DB(figure.location);

      var builder = Builders<DBLogicProcessor>.Filter;
      var filter =
        builder.GeoIntersects(t => t.figure.location, location)
        ;
      var list = await _collection.Find(filter)
        .ToListAsync();

      if (list == null)
      {
        return null;
      }

      return list.Select(p => p.logic_id).ToList();
    }

    public async Task Drop()
    {
      await _mongoDatabase.DropCollectionAsync(_tableName);

        _collection =
        _mongoDatabase.GetCollection<DBLogicProcessor>(
          _tableName
        );

      CreateIndexes();
    }

    public async Task Insert(GeoObjectDTO figure, string logic_id)
    {
      DBLogicProcessor processor = new DBLogicProcessor()
      {
        figure = ModelGate.ConvertDTO2DB(figure),
        logic_id = logic_id
      };

      await _collection.InsertOneAsync(processor);
    }
  }
}
