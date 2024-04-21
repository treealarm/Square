
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
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class EventsService: IEventsService, IDisposable
  {
    private IMongoCollection<DBEvent> _coll;
    private IMongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDatabaseSettings;
    private TableCursors<DBEvent> _cursors = new TableCursors<DBEvent>();
    private IMongoCollection<DBEvent> Coll
    {
      get
      {
        if (_coll == null)
        {
          CreateCollections();
        }        
        return _coll;
      }
    }
    public EventsService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient
    )
    {
      _geoStoreDatabaseSettings = geoStoreDatabaseSettings;
      _mongoClient = mongoClient;
      CreateCollections();
    }
    private void CreateCollections()
    {
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
                  .Ascending(d => d.id)
                  .Descending(d => d.id)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "id" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
      {
        IndexKeysDefinition<DBEvent> keys =
                new IndexKeysDefinitionBuilder<DBEvent>()
                  .Ascending(d => d.timestamp)
                  .Descending(d => d.timestamp)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "ts" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBEvent> keys =
                new IndexKeysDefinitionBuilder<DBEvent>()
                  .Ascending(d => d.object_id)
                  .Descending(d => d.object_id)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "oid" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBEvent> keys =
                new IndexKeysDefinitionBuilder<DBEvent>()
                  .Ascending(d => d.event_name)
                  .Descending(d => d.event_name)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "en" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }

      {
        IndexKeysDefinition<DBEvent> keys =
                new IndexKeysDefinitionBuilder<DBEvent>()
                  .Ascending(d => d.event_priority)
                  .Descending(d => d.event_priority)
                ;

        var indexModel = new CreateIndexModel<DBEvent>(
          keys, new CreateIndexOptions()
          { Name = "epr" }
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
        //timestamp = db_event.timestamp,
      };
      db_event.CopyAllTo(dto);
      dto.meta.extra_props = ModelGate.ConverDBExtraProp2DTO(db_event.meta.extra_props);
      dto.meta.not_indexed_props = ModelGate.ConverDBExtraProp2DTO(db_event.meta.not_indexed_props);
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
        if (string.IsNullOrEmpty(ev.id))
        {
          ev.id = ObjectId.GenerateNewId().ToString();
        }
        if (string.IsNullOrEmpty(ev.object_id))
        {
          ev.object_id = null;
        }
        var dbTrack = new DBEvent()
        {
          //timestamp = ev.timestamp
        };

        ev.CopyAllTo(dbTrack);
        dbTrack.meta.extra_props = ModelGate.ConvertExtraPropsToDB(ev.meta.extra_props);
        dbTrack.meta.not_indexed_props = ModelGate.ConvertExtraPropsToDB(ev.meta.not_indexed_props);
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

    private FilterDefinition<DBEvent> CreateOrAddFilter(FilterDefinition<DBEvent> filter, FilterDefinition<DBEvent> filter2add)
    {
      if (filter == null || filter == FilterDefinition<DBEvent>.Empty)
      {
        filter = filter2add;
      }
      else
      {
        filter = filter & filter2add;
      }
      return filter;
    }
    public async Task<List<EventDTO>> GetEventsByFilter(SearchFilterDTO filter_in)
    {
      var storedCursor = _cursors.Get(filter_in.search_id, filter_in.GetHashCode());

      if (storedCursor != null)
      {
        if (filter_in.forward == 0)
        {
          // Home.
          _cursors.Remove(filter_in.search_id);
        } 
        else
        {
          storedCursor.ReserveSeconds(60);
          List<DBEvent> prevList = null;
          if (storedCursor.Cursor?.Current !=null)
          {
            prevList = storedCursor.Cursor.Current.ToList();
          }

          bool available = false;
          // if not forward then just update time.
          if (filter_in.forward > 0)
          {
            available = await storedCursor.Cursor.MoveNextAsync();
          }          

          if (!available)
          {
            return DBListToDTO(prevList);
          }
          return DBListToDTO(storedCursor.Cursor.Current.ToList());
        }
      }
      int limit = 10000;

      if (filter_in.count > 0)
      {
        limit = filter_in.count;
      }

      var builder = Builders<DBEvent>.Filter;

      FilterDefinition<DBEvent> filter = FilterDefinition <DBEvent>.Empty;

      if (filter_in.time_start !=  null)
      {
        var fts = builder
        .Where(t => t.timestamp >= filter_in.time_start);
        filter = CreateOrAddFilter(filter, fts);
      }

      if (filter_in.time_end != null)
      {
        var fte = builder
        .Where(t => t.timestamp <= filter_in.time_end);
        filter = CreateOrAddFilter(filter, fte);
      }

      if (!string.IsNullOrEmpty(filter_in.start_id))
      {
        var fte = builder
        .Where(t => t.event_name.Contains(filter_in.start_id));
        filter = CreateOrAddFilter(filter, fte);
      }

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

          filter = CreateOrAddFilter(filter, f1);
        }
      }

      var options = new FindOptions()
      {
        BatchSize = limit
      };
      var finder = Coll.Find(filter, options);

      List<SortDefinition<DBEvent>> sorts = new List<SortDefinition<DBEvent>>();

      if (filter_in.sort != null && filter_in.sort.Count > 0)
      {
        var keys = filter_in.sort.Select(i=>i.key).ToList();
        keys.Sort();        

        var sortDefinitionBuilder = new SortDefinitionBuilder<DBEvent>();
        foreach (var kvp in filter_in.sort)
        {
          var k = kvp.key;
          //if (k != "timestamp")
          //{
          //  k = $"{"meta".kvp.key}";
          //}
           var sort = kvp.order == "asc" ? sortDefinitionBuilder.Ascending(k) : sortDefinitionBuilder.Descending(k);
           sorts.Add(sort);
        }
        finder = finder.Sort(sortDefinitionBuilder.Combine(sorts.ToArray()));
      }

      if (string.IsNullOrEmpty(filter_in.search_id))
      {
        finder = finder.Limit(limit);
        return DBListToDTO(await finder.ToListAsync());
      }
      var newCursor = await finder.ToCursorAsync();      
      bool available2 = await newCursor.MoveNextAsync();

      if (!available2)
      {
        newCursor.Dispose();
        return new List<EventDTO>();
      }
      _cursors.Add(filter_in.search_id, filter_in.GetHashCode(), newCursor);
      return DBListToDTO(newCursor.Current.ToList());
    }

    public void Dispose()
    {
      _cursors.Dispose();
    }

    public Task<long> ReserveCursor(string search_id)
    {
      var storedCursor = _cursors.GetById(search_id);

      if (storedCursor != null)
      {
        storedCursor.ReserveSeconds(60);
        return Task.FromResult(0L);
      }
      return Task.FromResult(-1L);
    }
  }
}
