
using DbLayer.Models;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class EventsService: IEventsService, IDisposable
  {
    private IMongoCollection<DBEvent> _coll;
    private IMongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDatabaseSettings;
    private readonly IGroupsService _groupsService;
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
      IMongoClient mongoClient,
      IGroupsService groupsService
    )
    {
      _geoStoreDatabaseSettings = geoStoreDatabaseSettings;
      _mongoClient = mongoClient;
      _groupsService = groupsService;
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

          //var timeField = nameof(DBEvent.timestamp);
          //var metaField = nameof(DBEvent.meta);

          //createOptions.TimeSeriesOptions =
          //  new TimeSeriesOptions(timeField, metaField, TimeSeriesGranularity.Seconds);

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

      // Вспомогательная функция для создания индекса
      void CreateIndex(IndexKeysDefinition<DBEvent> keys, string indexName)
      {
        var indexModel = new CreateIndexModel<DBEvent>(keys, new CreateIndexOptions { Name = indexName });
        _coll.Indexes.CreateOne(indexModel);
      }

      // Индекс для поля timestamp (по возрастанию и убыванию)
      CreateIndex(
          Builders<DBEvent>.IndexKeys.Ascending(d => d.timestamp),
          "ts_asc"
      );
      CreateIndex(
          Builders<DBEvent>.IndexKeys.Descending(d => d.timestamp),
          "ts_desc"
      );

      // Индекс для текстового поиска по event_name (по возрастанию и убыванию)
      CreateIndex(
          Builders<DBEvent>.IndexKeys.Ascending(d => d.event_name),
          "en_asc"
      );
      CreateIndex(
          Builders<DBEvent>.IndexKeys.Descending(d => d.event_name),
          "en_desc"
      );

      // Индекс для поля object_id (по возрастанию и убыванию)
      CreateIndex(
          new IndexKeysDefinitionBuilder<DBEvent>().Ascending(d => d.object_id),
          "oid_asc"
      );
      CreateIndex(
          new IndexKeysDefinitionBuilder<DBEvent>().Descending(d => d.object_id),
          "oid_desc"
      );

      // Индекс для поля event_priority (по возрастанию и убыванию)
      CreateIndex(
          new IndexKeysDefinitionBuilder<DBEvent>().Ascending(d => d.event_priority),
          "epr_asc"
      );
      CreateIndex(
          new IndexKeysDefinitionBuilder<DBEvent>().Descending(d => d.event_priority),
          "epr_desc"
      );

      // Индекс для поля meta.extra_props.prop_name и meta.extra_props.str_val

      var val1 = $"{nameof(DBEvent.meta)}.{nameof(DBEvent.meta.extra_props)}.{nameof(DBObjExtraProperty.prop_name)}";
      var val2 = $"{nameof(DBEvent.meta)}.{nameof(DBEvent.meta.extra_props)}.{nameof(DBObjExtraProperty.str_val)}";
      var indexModel = new CreateIndexModel<DBEvent>(
        Builders<DBEvent>.IndexKeys.Combine(
            Builders<DBEvent>.IndexKeys.Ascending(val1),
            Builders<DBEvent>.IndexKeys.Ascending(val2)
        ),
        new CreateIndexOptions
        {
          Name = "ep",
          Unique = false // Убедитесь, что индекс не уникальный, если не нужно
        }
      );
      _coll.Indexes.CreateOne(indexModel);
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

    private FilterDefinition<T> CreateOrAddFilter<T>(FilterDefinition<T> filter, FilterDefinition<T> filter2add)
    {
      if (filter == null || filter == FilterDefinition<T>.Empty)
      {
        // Если фильтр пустой или null, просто присваиваем новый фильтр
        return filter2add;
      }
      else
      {
        // Иначе соединяем старый и новый фильтры с помощью логического И (AND)
        return Builders<T>.Filter.And(filter, filter2add);
      }
    }


    public async Task<List<EventDTO>> GetEventsByFilter(SearchEventFilterDTO filter_in)
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

          if (!available && prevList != null)
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

      var objs2filter = filter_in.objects.ToList();

      if (filter_in.groups.Count > 0)
      {
        var objs = await _groupsService.GetListByNamesAsync(filter_in.groups);
        var ids = objs.Values.Select(t => t.objid).ToList();

        var ftg = builder
        .Where(t => ids.Contains(t.object_id));
        filter = CreateOrAddFilter(filter, ftg);
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
          if (string.IsNullOrEmpty(prop.prop_name))
          {
            continue;
          }

          if (string.IsNullOrEmpty(prop.str_val))
          {
            prop.str_val = string.Empty;
          }

          var f1 = Builders<DBEvent>.Filter.ElemMatch(
              t => t.meta.extra_props,
              Builders<DBObjExtraProperty>.Filter.And(
                  Builders<DBObjExtraProperty>.Filter.Eq(e => e.prop_name, prop.prop_name),
                  Builders<DBObjExtraProperty>.Filter.Eq(e => e.str_val, prop.str_val)
              )
          );

          // Объединяем этот фильтр с основным фильтром
          filter = CreateOrAddFilter(filter, f1);
        }
      }


      var ret_json = Utils.Log(filter);

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
