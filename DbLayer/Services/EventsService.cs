﻿
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

      {
        var keys = Builders<DBEvent>.IndexKeys.Combine(
          Builders<DBEvent>.IndexKeys
          .Ascending($"{nameof(DBEvent.meta.extra_props)}.{nameof(DBObjExtraProperty.prop_name)}"),
          Builders<DBEvent>.IndexKeys
          .Ascending($"{nameof(DBEvent.meta.extra_props)}.{nameof(DBObjExtraProperty.str_val)}"));

        var indexModel = new CreateIndexModel<DBEvent>(
           keys, new CreateIndexOptions()
           { Name = "ep" }
         );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
    }
    private EventDTO ConvertDB2DTO(DBEvent db_event)
    {
      if (db_event == null)
      {
        return null;
      }

      var dto = new EventDTO()
      {
        extra_props = ModelGate.ConverDBExtraProp2DTO(db_event.meta.extra_props),
        timestamp = db_event.timestamp,
      };
      db_event.meta.CopyAllTo(dto.meta);

      return dto;
    }
    private List<EventDTO> DBListToDTO(List<DBEvent> dbTracks)
    {
      List<EventDTO> list = new List<EventDTO>();
      foreach (var t in dbTracks)
      {
        var dto = ConvertDB2DTO(t);
        list.Add(dto);
      }
      return list;
    }
    async Task<long> IEventsService.InsertManyAsync(List<EventDTO> events)
    {
      List<DBEvent> list = new List<DBEvent>();

      foreach (var ev in events)
      {
        if (string.IsNullOrEmpty(ev.meta.id))
        {
          ev.meta.id = ObjectId.GenerateNewId().ToString();
        }
        var dbTrack = new DBEvent()
        {
          timestamp = ev.timestamp
        };

        ev.meta.CopyAllTo(dbTrack.meta);
        dbTrack.meta.extra_props = ModelGate.ConvertExtraPropsToDB(ev.extra_props);
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
          Console.WriteLine(ex.ToString());
          throw;
        }
        
      }

      return list.Count;
    }
    public async Task<List<EventDTO>> GetEventsByFilter(SearchFilterDTO filter_in)
    {
      int limit = 10000;

      if (filter_in.count > 0)
      {
        limit = filter_in.count;
      }

      var builder = Builders<DBEvent>.Filter;

      FilterDefinition<DBEvent> filter =
        builder
        .Where(t => t.timestamp >= filter_in.time_start
          && t.timestamp <= filter_in.time_end);


      if (!string.IsNullOrEmpty(filter_in.start_id))
      {
        FilterDefinition<DBEvent> filterPaging = null;

        if (filter_in.forward)
          filterPaging = Builders<DBEvent>.Filter
            .Gt("meta._id", new ObjectId(filter_in.start_id));
        else
          filterPaging = Builders<DBEvent>.Filter
            .Lt("meta._id", new ObjectId(filter_in.start_id));

        filter = filter & filterPaging;
      }


      var dbObjects = new List<DBEvent>();

      if (filter_in.property_filter != null && filter_in.property_filter.props.Count > 0)
      {
        foreach (var prop in filter_in.property_filter.props)
        {
          var request =
            string.Format("{{prop_name:'{0}', str_val:'{1}'}}",
            prop.prop_name,
            prop.str_val);

          var f1 = Builders<DBEvent>
            .Filter
            .ElemMatch(t => t.meta.extra_props, request)
            ;

          var metaValue = new BsonDocument(
              "str_val",
              prop.str_val
              );

          filter &= f1;
        }
      }

      var finder = Coll.Find(filter).Limit(limit);


      var list = await finder
        .ToListAsync();

      dbObjects.AddRange(list);

      return DBListToDTO(dbObjects);
    }
  }
}