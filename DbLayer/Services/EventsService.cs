
using DbLayer.Models;
using DbLayer.Models.Events;
using Domain;
using Domain.Events;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class EventsService: IEventsService
  {
    private IMongoCollection<DBEvent> _coll;
    private MongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDatabaseSettings;
    private IMongoCollection<DBEvent> Coll
    {
      get
      {
        CreateCollections();
        return _coll;
      }
    }
    public EventsService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings
    )
    {
      _geoStoreDatabaseSettings = geoStoreDatabaseSettings;
      CreateCollections();
    }
    private void CreateCollections()
    {
      _mongoClient = new MongoClient(
       _geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          _geoStoreDatabaseSettings.Value.DatabaseName);

      var EventsCollectionName = _geoStoreDatabaseSettings.Value.EventsCollectionName;

      var filter = new BsonDocument("name", EventsCollectionName);
      var options = new ListCollectionNamesOptions { Filter = filter };

      try
      {
        if (!mongoDatabase.ListCollectionNames(options).Any())
        {
          var createOptions = new CreateCollectionOptions();

          var timeField = nameof(DBEvent.timestamp);
          var metaField = nameof(DBEvent.meta);

          createOptions.TimeSeriesOptions =
            new TimeSeriesOptions(timeField, metaField, TimeSeriesGranularity.Seconds);

          mongoDatabase.CreateCollection(
          EventsCollectionName,
          createOptions);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      _coll =
        mongoDatabase.GetCollection<DBEvent>(
          EventsCollectionName
        );

      CreateIndexes();
    }
    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBEvent> keys =
                new IndexKeysDefinitionBuilder<DBEvent>()
                .Ascending(d => d.meta.id)
                .Ascending(d => d.timestamp)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "combo" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
      {
        IndexKeysDefinition<DBEvent> keys =
                new IndexKeysDefinitionBuilder<DBEvent>()
                  .Ascending(d => d.meta.id)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "mid" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }

      //{
      //  var keys = Builders<DBEvent>.IndexKeys.Combine(
      //    Builders<DBEvent>.IndexKeys
      //    .Ascending($"{nameof(DBEvent.extra_props)}.{nameof(DBObjExtraProperty.prop_name)}"),
      //    Builders<DBEvent>.IndexKeys
      //    .Ascending($"{nameof(DBEvent.extra_props)}.{nameof(DBObjExtraProperty.str_val)}"));

      //  var indexModel = new CreateIndexModel<DBEvent>(
      //     keys, new CreateIndexOptions()
      //     { Name = "ep" }
      //   );

      //  _coll.Indexes.CreateOneAsync(indexModel);
      //}
    }

    async Task<long> IEventsService.InsertManyAsync(List<EventDTO> events)
    {
      List<DBEvent> list = new List<DBEvent>();

      foreach (var ev in events)
      {
        if (string.IsNullOrEmpty(ev.meta.id))
        {
          ev.meta.id = "000000000000000000000000";
        }
        var dbTrack = new DBEvent()
        {
          timestamp = ev.timestamp
        };

        ev.meta.CopyAllTo(dbTrack.meta);
        dbTrack.extra_props = ModelGate.ConvertExtraPropsToDB(ev.extra_props);
        list.Add(dbTrack);
      }

      if (list.Count > 0)
      {
        try
        {
          await Coll.InsertManyAsync(list);
        }
        catch (Exception ex)
        {
          throw;
        }
        
      }

      return list.Count;
    }
  }
}
