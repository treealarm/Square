using DbLayer.Models;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class StateService: IStateService
  {
    private readonly IMongoCollection<DBObjectState> _collState;
    private readonly IMongoCollection<DBObjectStateDescription> _collStateDescr;
    private readonly MongoClient _mongoClient;
    public StateService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _collState =
        mongoDatabase.GetCollection<DBObjectState>
        (geoStoreDatabaseSettings.Value.StateCollectionName);
      _collStateDescr =
        mongoDatabase.GetCollection<DBObjectStateDescription>
        (geoStoreDatabaseSettings.Value.StateDescrCollectionName);
    }

    public async Task Init()
    {
      var something = await _collStateDescr.AsQueryable().FirstOrDefaultAsync();

      if (something == null)
      {
        List<ObjectStateDescriptionDTO> newObjs = new List<ObjectStateDescriptionDTO>();
        ObjectStateDescriptionDTO 
        pObj = new ObjectStateDescriptionDTO()
        {
          state = "ALARM",
          state_descr = "Alarm",
          state_color = "#FF0000",
          alarm = true
        };
        newObjs.Add(pObj);

        pObj = new ObjectStateDescriptionDTO()
        {
          state = "INFO",
          state_descr = "Info",
          state_color = "#00FF00",
          alarm = false
        };
        newObjs.Add(pObj);

        pObj = new ObjectStateDescriptionDTO()
        {
          state = "NORM",
          state_descr = "Normal",
          state_color = "#0000FF",
          alarm = false
        };
        newObjs.Add(pObj);

        await UpdateStateDescrsAsync(newObjs);
      }
      await Task.Delay(0);
    }

    public async Task UpdateStatesAsync(List<ObjectStateDTO> newObjs)
    {
      var objsToUpdate = ConvertListStateDTO2DB(newObjs);
      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;

      foreach (var updatedObj in objsToUpdate)
      {
        await _collState.ReplaceOneAsync(x => x.id == updatedObj.id, updatedObj, opt);
      }
    }
    public async Task UpdateStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs)
    {
      var objsToUpdate = ConvertListStateDescrDTO2DB(newObjs);

      foreach (var updatedObj in objsToUpdate)
      {
        await _collStateDescr.DeleteOneAsync(
          x => x.state == updatedObj.state && x.external_type == updatedObj.external_type
        );
      }

      await _collStateDescr.InsertManyAsync(objsToUpdate);
    }

    List<ObjectStateDTO> ConvertListStateDB2DTO(List<DBObjectState> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new List<ObjectStateDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new ObjectStateDTO()
        {
          id = dbObj.id,
          states = dbObj.states
        };
        newObjs.Add(dto);
      }
      return newObjs;
    }

    List<DBObjectState> ConvertListStateDTO2DB(List<ObjectStateDTO> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new List<DBObjectState>();

      foreach (var dbObj in dbo)
      {
        var dto = new DBObjectState()
        {
          id = dbObj.id,
          states = dbObj.states
        };
        newObjs.Add(dto);
      }
      return newObjs;
    }

    List<ObjectStateDescriptionDTO> ConvertListStateDescrDB2DTO(
      List<DBObjectStateDescription> dbo
    )
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new List<ObjectStateDescriptionDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new ObjectStateDescriptionDTO()
        {
          id = dbObj.id,
          state = dbObj.state,
          state_color = dbObj.state_color.ToString(),
          state_descr = dbObj.state_descr,
          alarm = dbObj.alarm,
          external_type = dbObj.external_type
        };
        newObjs.Add(dto);
      }
      return newObjs;
    }

    List<DBObjectStateDescription> ConvertListStateDescrDTO2DB(
       List<ObjectStateDescriptionDTO> dbo
     )
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new List<DBObjectStateDescription>();

      foreach (var dbObj in dbo)
      {
        var dto = new DBObjectStateDescription()
        {
          id = dbObj.id,
          state = dbObj.state,
          state_color = dbObj.state_color,
          state_descr = dbObj.state_descr,
          alarm = dbObj.alarm,
          external_type = dbObj.external_type
        };
        newObjs.Add(dto);
      }
      return newObjs;
    }

    public async Task<List<ObjectStateDTO>> GetStatesByTimeAsync(
      DateTime from_timestamp,
      int nLimit
    )
    {
      List<DBObjectState> obj = null;

      try
      {
        obj = await _collState
          .Find(s => s.timestamp > from_timestamp)
          .Limit(nLimit)
          .ToListAsync();
      }
      catch (Exception)
      {

      }

      return ConvertListStateDB2DTO(obj);
    }

    public async Task<List<ObjectStateDTO>> GetStatesAsync(List<string> ids)
    {
      List<DBObjectState> obj = null;

      try
      {
        obj = await _collState.Find(s => ids.Contains(s.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ConvertListStateDB2DTO(obj);
    }

    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(
      string external_type,
      List<string> states
    )
    {
      List<DBObjectStateDescription> obj = null;

      try
      {
        var f1 = Builders<DBObjectStateDescription>.Filter.Exists(external_type, false)
              | Builders<DBObjectStateDescription>.Filter.Eq(el => el.external_type, null)
              | Builders<DBObjectStateDescription>.Filter.Eq(el => el.external_type, String.Empty);

        if (states == null || states.Count == 0)
        {
          if (string.IsNullOrEmpty(external_type))
          {
            obj = await _collStateDescr
            .Find(f1)
            .ToListAsync();
          }
          else
          {
            obj = await _collStateDescr
            .Find(t => t.external_type == external_type)
            .ToListAsync();
          }          
        }
        else
        {
          if (string.IsNullOrEmpty(external_type))
          {
            var builder = Builders<DBObjectStateDescription>.Filter;
            var filter = builder.Where(el => states.Contains(el.state));            

            filter = filter & f1;

            obj = await _collStateDescr
            .Find(filter)
            .ToListAsync();
          }
          else
          {
            obj = await _collStateDescr
            .Find(s => s.external_type == external_type && states.Contains(s.state))
            .ToListAsync();
          }          
        }        
      }
      catch (Exception)
      {

      }
      return ConvertListStateDescrDB2DTO(obj);
    }
  }
}
