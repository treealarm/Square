using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class PgEventsService: IEventsService
  {
    private readonly PgDbContext _dbContext;
    private readonly IGroupsService _groupsService;

    public PgEventsService(PgDbContext context, IGroupsService groupsService)
    {
      _dbContext = context;
      _groupsService = groupsService;
    }

    public static string GenerateObjectId()
    {
      byte[] bytes = new byte[12];
      RandomNumberGenerator.Fill(bytes); // Заполняем случайными байтами
      return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // Преобразуем в строку hex
    }

    public static List<ObjExtraPropertyDTO> ConverDBExtraProp2DTO(List<PgDBObjExtraProperty> props)
    {
      var retVal = new List<ObjExtraPropertyDTO>();

      if (props == null)
      {
        return retVal;
      }

      foreach (var prop in props)
      {
        ObjExtraPropertyDTO newProp = new ObjExtraPropertyDTO()
        {
          prop_name = prop.prop_name,
          str_val = prop.str_val.ToString(),
          visual_type = prop.visual_type
        };
        retVal.Add(newProp);
      }

      return retVal;
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
      dto.id = Utils.ConvertGuidToObjectId(db_event.id);
      dto.object_id = Utils.ConvertGuidToObjectId(db_event.object_id);
      dto.extra_props = ConverDBExtraProp2DTO(db_event.extra_props);
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
    public static List<PgDBObjExtraProperty> ConvertExtraPropsToDB(
      List<ObjExtraPropertyDTO> extra_props, 
      Guid owner_id)
    {
      if (extra_props == null)
      { return null; }

      var ep_db = new List<PgDBObjExtraProperty>();
      var propertieNames = typeof(FigureZoomedDTO).GetProperties().Select(x => x.Name).ToList();

      propertieNames.AddRange(
        typeof(FigureGeoDTO).GetProperties().Select(x => x.Name)
        );


      foreach (var prop in extra_props)
      {
        // "radius", "zoom_level"
        if (propertieNames.Contains(prop.prop_name))
        {
          continue;
        }

        var newProp = new PgDBObjExtraProperty()
        {
          prop_name = prop.prop_name,
          visual_type = prop.visual_type,
          owner_id = owner_id,
        };

        newProp.id = Utils.ConvertObjectIdToGuid(ObjectId.GenerateNewId().ToString());

        if (prop.visual_type == BsonType.Double.ToString())
        {
          if (double.TryParse(
            prop.str_val,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var result))
          {
            newProp.str_val = result.ToString();
          }
        }
        else if (prop.visual_type == BsonType.DateTime.ToString())
        {
          newProp.str_val = DateTime
              .Parse(prop.str_val)
              .ToUniversalTime().ToString();
        }

        if (newProp.str_val == null)
        {
          newProp.str_val = prop.str_val;
        }
        ep_db.Add(newProp);
      }
      return ep_db;
    }




  public async Task<long> InsertManyAsync(List<EventDTO> events)
    {
      var list = new List<DBEvent>();

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
        var dbTrack = new DBEvent();

        ev.CopyAllTo(dbTrack);
        dbTrack.id = Utils.ConvertObjectIdToGuid(ev.id);
        dbTrack.object_id = Utils.ConvertObjectIdToGuid(ev.object_id);
        dbTrack.extra_props = ConvertExtraPropsToDB(ev.extra_props, dbTrack.id);
        list.Add(dbTrack);
      }

      if (list.Count > 0)
      {
        try
        {
          await _dbContext.Events.AddRangeAsync(list);
          await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
          throw;
        }

      }

      return list.Count;
    }

    public async Task<List<EventDTO>> GetEventsByFilter(SearchEventFilterDTO filter_in)
    {
      var query = _dbContext.Events.AsQueryable();

      if (filter_in.time_start != null)
      {
        query = query.Where(e => e.timestamp >= filter_in.time_start);
      }

      if (filter_in.time_end != null)
      {
        query = query.Where(e => e.timestamp <= filter_in.time_end);
      }

      if (filter_in.groups.Count > 0)
      {
        var objs = await _groupsService.GetListByNamesAsync(filter_in.groups);
        var ids = objs.Values.Select(t => Utils.ConvertObjectIdToGuid(t.objid)).ToList();
        query = query.Where(e => ids.Contains(e.object_id));
      }

      if (!string.IsNullOrEmpty(filter_in.start_id))
      {
        query = query.Where(e => e.event_name.Contains(filter_in.start_id));
      }

      if (filter_in.images_only)
      {
        query = query.Where(e => e.extra_props.Where(p=>p.visual_type == "image_fs").Any());
      }

      if (filter_in.property_filter != null && filter_in.property_filter.props.Count > 0)
      {
        foreach (var prop in filter_in.property_filter.props)
        {
          if (!string.IsNullOrEmpty(prop.prop_name))
          {
            var strVal = prop.str_val ?? string.Empty;
            query = query.Where(e => e.extra_props.Any(p => p.prop_name == prop.prop_name && p.str_val == strVal));
          }
        }
      }

      if (filter_in.sort != null && filter_in.sort.Count > 0)
      {
        IOrderedQueryable<DBEvent> orderedQuery = null;
        foreach (var kvp in filter_in.sort)
        {
          if (orderedQuery == null)
          {
            orderedQuery = kvp.order == "asc"
                ? query.OrderBy(e => EF.Property<object>(e, kvp.key))
                : query.OrderByDescending(e => EF.Property<object>(e, kvp.key));
          }
          else
          {
            orderedQuery = kvp.order == "asc"
                ? orderedQuery.ThenBy(e => EF.Property<object>(e, kvp.key))
                : orderedQuery.ThenByDescending(e => EF.Property<object>(e, kvp.key));
          }
        }

        if (orderedQuery != null)
        {
          query = orderedQuery;
        }
      }

      int limit = filter_in.count > 0 ? filter_in.count : 10000;
      query = query.Take(limit).Include(e => e.extra_props);

      return DBListToDTO(await query.ToListAsync());
    }
  }
}
