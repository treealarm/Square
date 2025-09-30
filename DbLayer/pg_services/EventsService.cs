using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class EventsService: IEventsService
  {
    private readonly PgDbContext _dbContext;
    private readonly IGroupsService _groupsService;

    public EventsService(PgDbContext context, IGroupsService groupsService)
    {
      _dbContext = context;
      _groupsService = groupsService;
    }

    public static List<ObjExtraPropertyDTO> ConverDBExtraProp2DTO(List<EventProp> props)
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
      dto.id = Domain.Utils.ConvertGuidToObjectId(db_event.id);
      dto.object_id = Domain.Utils.ConvertGuidToObjectId(db_event.object_id);
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
    public static List<EventProp> ConvertExtraPropsToDB(
        List<ObjExtraPropertyDTO> extra_props,
        Guid owner_id)
    {
      if (extra_props == null)
        return null;

      var ep_db = new List<EventProp>();

      // Игнорируем свойства встроенных DTO
      var propertyNames = typeof(FigureZoomedDTO).GetProperties().Select(p => p.Name).ToHashSet();
      propertyNames.UnionWith(typeof(FigureGeoDTO).GetProperties().Select(p => p.Name));

      foreach (var prop in extra_props)
      {
        if (propertyNames.Contains(prop.prop_name))
          continue;

        var newProp = new EventProp
        {
          id = Domain.Utils.NewGuid(),
          prop_name = prop.prop_name,
          visual_type = prop.visual_type,
          owner_id = owner_id
        };

        // Сохраняем значение как строку для jsonb
        if (prop.visual_type == "Double")
        {
          if (double.TryParse(prop.str_val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
            newProp.str_val = dbl.ToString(CultureInfo.InvariantCulture);
          else
            newProp.str_val = prop.str_val;
        }
        else if (prop.visual_type == "DateTime")
        {
          if (DateTime.TryParse(prop.str_val, out var dt))
            newProp.str_val = dt.ToUniversalTime().ToString("o"); // ISO8601
          else
            newProp.str_val = prop.str_val;
        }
        else
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
          ev.id = Domain.Utils.ConvertGuidToObjectId(Domain.Utils.NewGuid());
        }
        if (string.IsNullOrEmpty(ev.object_id))
        {
          ev.object_id = null;
        }
        var dbTrack = new DBEvent();

        ev.CopyAllTo(dbTrack);
        dbTrack.id = Domain.Utils.ConvertObjectIdToGuid(ev.id) ?? throw new InvalidOperationException("ConvertObjectIdToGuid");
        dbTrack.object_id = Domain.Utils.ConvertObjectIdToGuid(ev.object_id) ?? throw new InvalidOperationException("ConvertObjectIdToGuid");
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

    private static object GetPropertyValue(object obj, string propertyName)
    {
      return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
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
        var ids = objs.Values.Select(t => Domain.Utils.ConvertObjectIdToGuid(t.objid)).ToList();
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

      if (filter_in.param0 != null)
      {
        query = query.Where(e => e.param0 == filter_in.param0);
      }
      if (filter_in.param1 != null)
      {
        query = query.Where(e => e.param1 == filter_in.param1);
      }

      if (filter_in.sort != null)
      {
        var timestampSort = filter_in.sort.FirstOrDefault(s => s.key == "timestamp");
        if (timestampSort != null)
        {
          query = timestampSort.order == "asc"
              ? query.OrderBy(e => e.timestamp)
              : query.OrderByDescending(e => e.timestamp);
        }
      }

      int limit = filter_in.count > 0 ? filter_in.count : 10000;
      query = query
        .Skip(limit* filter_in.forward)
        .Take(limit)
        .Include(e => e.extra_props);

      var sql = query.ToQueryString();
      Console.WriteLine(sql);

      var result = DBListToDTO(await query.ToListAsync());

      IOrderedEnumerable<EventDTO> ordered = null;

      foreach (var sort in filter_in.sort)
      {
        Func<EventDTO, object> keySelector = x => GetPropertyValue(x, sort.key);

        if (ordered == null)
        {
          ordered = sort.order == "asc"
              ? result.OrderBy(keySelector)
              : result.OrderByDescending(keySelector);
        }
        else
        {
          ordered = sort.order == "asc"
              ? ordered.ThenBy(keySelector)
              : ordered.ThenByDescending(keySelector);
        }
      }

      if (ordered != null)
      {
        result = ordered.ToList();
      }


      return result;
    }
  }
}
