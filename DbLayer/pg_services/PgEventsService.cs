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

    public PgEventsService(PgDbContext context)
    {
      _dbContext = context;
    }

    public static string GenerateObjectId()
    {
      byte[] bytes = new byte[12];
      RandomNumberGenerator.Fill(bytes); // Заполняем случайными байтами
      return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // Преобразуем в строку hex
    }

    public static string ConvertIntToObjectId(int id)
    {
      byte[] bytes = new byte[12];
      BitConverter.GetBytes(id).CopyTo(bytes, 8); // Заполняем последние 4 байта
      RandomNumberGenerator.Fill(bytes.AsSpan(0, 8)); // Заполняем остальные 8 байт случайными данными
      return BitConverter.ToString(bytes).Replace("-", "").ToLower();
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
          str_val = prop.str_val.ToString()
        };
        retVal.Add(newProp);
      }

      return retVal;
    }
    private EventDTO ConvertDB2DTO(PgDBEvent db_event)
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
      dto.extra_props = ConverDBExtraProp2DTO(db_event.extra_props);
      return dto;
    }
    private List<EventDTO> DBListToDTO(List<PgDBEvent> dbTracks)
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

        newProp.id = ConvertObjectIdToGuid(ObjectId.GenerateNewId().ToString());

        var s = BsonType.Double.ToString();
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
        else
        if (prop.visual_type == BsonType.DateTime.ToString())
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

  public static Guid ConvertObjectIdToGuid(string objectIdString)
  {
    if (objectIdString.Length != 24)
      throw new ArgumentException("Invalid ObjectId length", nameof(objectIdString));

    // Преобразуем строку ObjectId в массив байтов (каждый символ - это 4 бита, 24 символа = 12 байт)
    byte[] objectIdBytes = new byte[12];
    for (int i = 0; i < 12; i++)
    {
      objectIdBytes[i] = Convert.ToByte(objectIdString.Substring(i * 2, 2), 16);
    }

    // Преобразуем первые 16 байт в UUID (если необходимо, добавьте дополнительные байты)
    byte[] uuidBytes = new byte[16];
    Array.Copy(objectIdBytes, uuidBytes, 12); // Копируем первые 12 байт

    return new Guid(uuidBytes);
  }

  public static string ConvertGuidToObjectId(Guid guid)
  {
    byte[] guidBytes = guid.ToByteArray();
    byte[] objectIdBytes = new byte[12];
    Array.Copy(guidBytes, objectIdBytes, 12); // Копируем первые 12 байт

    // Преобразуем байты обратно в строку
    string objectIdString = BitConverter.ToString(objectIdBytes).Replace("-", "").ToLower();
    return objectIdString;
  }

  public async Task<long> InsertManyAsync(List<EventDTO> events)
    {
      var list = new List<PgDBEvent>();

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
        var dbTrack = new PgDBEvent();

        ev.CopyAllTo(dbTrack);
        dbTrack.id = ConvertObjectIdToGuid(ev.id);
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
      var eventsWithProps = await _dbContext.Events
        .Include(e => e.extra_props)  // Подключаем связанные данные
        .Take(1000)
        .ToListAsync();
      return DBListToDTO(eventsWithProps);
    }

    public async Task<long> ReserveCursor(string search_id)
    {
      //throw new NotImplementedException();
      return 0;
    }
  }
}
