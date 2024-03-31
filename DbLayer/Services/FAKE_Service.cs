using DbLayer.Models;
using Domain.GeoDTO;
using Domain.OptionsModels;
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
  public class FAKE_Service: ILogicProcessorService
  {
    private IMongoCollection<DB__FAKE> _collection;
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDatabase;
    private string _tableName;
    public FAKE_Service(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient)
    {
      _mongoClient = mongoClient;

      _mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _tableName = geoStoreDatabaseSettings.Value.LogicProcessorCollectionName;
      _collection =
        _mongoDatabase.GetCollection<DB__FAKE>(
          _tableName
          );

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DB__FAKE> keys =
          new IndexKeysDefinitionBuilder<DB__FAKE>()
          .Geo2DSphere(d => d.figure.location)
          ;

        var indexModel = new CreateIndexModel<DB__FAKE>(
          keys, new CreateIndexOptions()
          { Name = "geo" }
        );

        _collection.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task<List<string>> GetLogicByFigure(GeoObjectDTO figure)
    {
      var location = ModelGate.ConvertGeoDTO2DB(figure.location);

      var builder = Builders<DB__FAKE>.Filter;
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
        _mongoDatabase.GetCollection<DB__FAKE>(
          _tableName
        );

      CreateIndexes();
    }

    public async Task Insert(GeoObjectDTO figure, string logic_id)
    {
      DB__FAKE processor = new DB__FAKE()
      {
        figure = ModelGate.ConvertDTO2DB(figure),
        logic_id = logic_id
      };

      await _collection.InsertOneAsync(processor);
    }
  }
}
