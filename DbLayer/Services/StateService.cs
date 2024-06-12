using DbLayer.Models;
using Domain;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Domain.States;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class StateService: IStateService
  {
    private IMongoCollection<DBObjectState> _collState;
    private IMongoCollection<DBObjectStateDescription> _collStateDescr;
    private IMongoCollection<DBAlarmState> _collAlarms;
    private IMongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDatabaseSettings;
    
    private IMongoCollection<DBObjectState> CollState 
    { 
      get
      {
        CreateCollections();
        return _collState;
      }
    }

    private IMongoCollection<DBObjectStateDescription> CollStateDescr
    {
      get
      {
        CreateCollections();
        return _collStateDescr;
      }
    }

    private IMongoCollection<DBAlarmState> CollAlarms
    {
      get
      {
        CreateCollections();
        return _collAlarms;
      }
    }

    public StateService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient)
    {
      _geoStoreDatabaseSettings = geoStoreDatabaseSettings;
      _mongoClient = mongoClient;
      CreateCollections();
    }
    private void CreateCollections()
    {
      if (_collState == null || _collStateDescr == null || _collAlarms == null)
      {
        var mongoDatabase = _mongoClient.GetDatabase(
            _geoStoreDatabaseSettings.Value.DatabaseName);

        _collState =
          mongoDatabase.GetCollection<DBObjectState>
          (_geoStoreDatabaseSettings.Value.StateCollectionName);

        _collStateDescr =
          mongoDatabase.GetCollection<DBObjectStateDescription>
          (_geoStoreDatabaseSettings.Value.StateDescrCollectionName);

        _collAlarms =
          mongoDatabase.GetCollection<DBAlarmState>
          (_geoStoreDatabaseSettings.Value.StateAlarmsCollectionName);

        CreateIndexes();
      }
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBObjectStateDescription> keys =
                new IndexKeysDefinitionBuilder<DBObjectStateDescription>()
                .Ascending(d => d.external_type)
                .Ascending(d => d.alarm)
                .Ascending(d => d.state)
                ;

        var indexModel = new CreateIndexModel<DBObjectStateDescription>(
          keys, new CreateIndexOptions()
          { Name = "state" }
        );

        CollStateDescr?.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task Init()
    {
      var something = await CollStateDescr?.AsQueryable().FirstOrDefaultAsync();

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

    public async Task<long> UpdateStatesAsync(List<ObjectStateDTO> newObjs)
    {
      var dbUpdated = new Dictionary<ObjectStateDTO, DBObjectState>();
      var bulkWrites = new List<WriteModel<DBObjectState>>();

      foreach (var item in newObjs)
      {
        var updatedObj = ConvertStateDTO2DB(item);
        dbUpdated.Add(item, updatedObj);
        var filter = Builders<DBObjectState>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          var request = new InsertOneModel<DBObjectState>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBObjectState>(filter, updatedObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        }
      }

      var writeResult = await CollState?.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
      }
      return writeResult.ModifiedCount;
    }
    public async Task<long> UpdateStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs)
    {
      var objsToUpdate = ConvertListStateDescrDTO2DB(newObjs);

      foreach (var updatedObj in objsToUpdate)
      {
        await CollStateDescr?.DeleteOneAsync(
          x => x.state == updatedObj.state && x.external_type == updatedObj.external_type
        );
      }

      await CollStateDescr?.InsertManyAsync(objsToUpdate);
      return (long)objsToUpdate.Count;
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

    DBObjectState ConvertStateDTO2DB(ObjectStateDTO dto)
    {
      if (dto == null)
      {
        return null;
      }

      var dbo = new DBObjectState()
      {
        id = dto.id,
        states = dto.states
      };

      return dbo;
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
          alarm = dbObj.alarm ?? false,
          external_type = dbObj.external_type
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
        obj = await CollState?.Find(s => ids.Contains(s.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ConvertListStateDB2DTO(obj);
    }

    Dictionary<string, List<ObjectStateDescriptionDTO>> _cashStateDescr = 
      new Dictionary<string, List<ObjectStateDescriptionDTO>>();
    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescrByExternalTypeAsync(
      string external_type
    )
    {
      List<DBObjectStateDescription> obj = null;

      var f1 = Builders<DBObjectStateDescription>.Filter.Exists(external_type, false)
              | Builders<DBObjectStateDescription>.Filter.Eq(el => el.external_type, null)
              | Builders<DBObjectStateDescription>.Filter.Eq(el => el.external_type, String.Empty);

      if (string.IsNullOrEmpty(external_type))
      {
        obj = await CollStateDescr?
        .Find(f1)
        .ToListAsync();
      }
      else
      {
        obj = await CollStateDescr?
        .Find(t => t.external_type == external_type)
        .ToListAsync();
      }

      return ConvertListStateDescrDB2DTO(obj);
    }

    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(
      string external_type,
      List<string> states
    )
    {
      var et = external_type;

      if (string.IsNullOrEmpty(et))
      {
        et = string.Empty;
      }

      lock (_cashStateDescr)
      {
        if (_cashStateDescr.TryGetValue(et, out var list))
        {
          if (states == null || states.Count == 0)
          {
            return list;
          }
          return list.Where(s => states.Contains(s.state)).ToList();
        }
      }      

      var temp = await GetStateDescrByExternalTypeAsync(external_type);

      lock (_cashStateDescr)
      {
        _cashStateDescr[et] = temp;
      }

      List<DBObjectStateDescription> obj = null;

      try
      {
        if (states == null || states.Count == 0)
        {
          return await GetStateDescrByExternalTypeAsync(external_type);
        }
        else
        {
          if (string.IsNullOrEmpty(external_type))
          {
            var f1 = Builders<DBObjectStateDescription>.Filter.Exists(external_type, false)
              | Builders<DBObjectStateDescription>.Filter.Eq(el => el.external_type, null)
              | Builders<DBObjectStateDescription>.Filter.Eq(el => el.external_type, String.Empty);

            var builder = Builders<DBObjectStateDescription>.Filter;
            var filter = builder.Where(el => states.Contains(el.state));            

            filter = filter & f1;

            obj = await CollStateDescr?
            .Find(filter)
            .ToListAsync();
          }
          else
          {
            obj = await CollStateDescr?
            .Find(s => s.external_type == external_type && states.Contains(s.state))
            .ToListAsync();
          }          
        }        
      }
      catch (Exception)
      {

      }
      var retVal = ConvertListStateDescrDB2DTO(obj);
      return retVal;
    }

    async Task IStateService.UpdateAlarmStatesAsync(List<AlarmState> alarms)
    {
      var bulkWrites = new List<WriteModel<DBAlarmState>>();

      foreach (var item in alarms)
      {
        var updatedObj = item.CopyAll<AlarmState, DBAlarmState>();
        var filter = Builders<DBAlarmState>.Filter.Eq(x => x.id, updatedObj.id);
        var request = new ReplaceOneModel<DBAlarmState>(filter, updatedObj);
        request.IsUpsert = true;
        bulkWrites.Add(request);
      }

      var writeResult = await CollAlarms?.BulkWriteAsync(bulkWrites);
    }

    async Task<Dictionary<string,AlarmState>> IStateService.GetAlarmStatesAsync(List<string> ids)
    {
      var retVal = new  Dictionary<string, AlarmState>  ();
      var list = await CollAlarms.Find(i => ids.Contains(i.id)).ToListAsync();

      foreach (var item in list)
      {
        retVal.Add(item.id, new AlarmState()
        {
          id = item.id,
          alarm = item.alarm
        });
      }
      return retVal;
    }
    async Task IStateService.DropStateAlarms()
    {
      if (CollAlarms != null)
      {
        await _collAlarms
        .Database
        .DropCollectionAsync(_geoStoreDatabaseSettings.Value.StateAlarmsCollectionName);
      }      
    }
  }
}
