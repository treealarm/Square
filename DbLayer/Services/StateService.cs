using DbLayer.Models;
using Domain;
using Domain.ServiceInterfaces;
using Domain.States;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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

    public async Task InsertStatesAsync(List<ObjectStateDTO> newObjs)
    {
      await _collState.InsertManyAsync(ConvertListStateDTO2DB(newObjs));
    }
    public async Task InsertStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs)
    {
      await _collStateDescr.InsertManyAsync(ConvertListStateDescrDTO2DB(newObjs));
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
          state_color = dbObj.state_color,
          state_descr = dbObj.state_descr,
          alarm = dbObj.alarm
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
          alarm =  dbObj.alarm
        };
        newObjs.Add(dto);
      }
      return newObjs;
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
      List<string> states
    )
    {
      List<DBObjectStateDescription> obj = null;

      try
      {
        if (states.Count == 0)
        {
          obj = await _collStateDescr.AsQueryable().ToListAsync();
        }
        else
        {
          obj = await _collStateDescr.Find(s => states.Contains(s.state)).ToListAsync();
        }        
      }
      catch (Exception)
      {

      }
      return ConvertListStateDescrDB2DTO(obj);
    }
  }
}
