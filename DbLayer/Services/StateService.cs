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
  internal class StateService: IStateService
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
          x => x.state == updatedObj.state
        );
      }

      await CollStateDescr?.InsertManyAsync(objsToUpdate);
      return (long)objsToUpdate.Count;
    }

    Dictionary<string, ObjectStateDTO> ConvertListStateDB2DTO(List<DBObjectState> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new Dictionary<string, ObjectStateDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new ObjectStateDTO()
        {
          id = dbObj.id,
          states = dbObj.states
        };
        newObjs.Add(dbObj.id, dto);
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
          alarm = dbObj.alarm ?? false
        };
        newObjs.Add(dto);
      }
      return newObjs;
    }

    public async Task<Dictionary<string, ObjectStateDTO>> GetStatesAsync(List<string> ids)
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

    public async Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(
      List<string> states
    )
    {
      List<DBObjectStateDescription> obj = null;

      try
      {
        obj = await CollStateDescr?
        .Find(s => states.Contains(s.state))
        .ToListAsync();

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

    //Task<Dictionary<string, ObjectStateDescriptionDTO>> IStateService.GetAlarmStatesDescr(List<string> statesFilter)
    //{
    //  throw new NotImplementedException();
    //}
    public async Task<Dictionary<string, ObjectStateDescriptionDTO>> GetAlarmStatesDescr(List<string> states)
    {
      // TODO make states unique.

      List<DBObjectStateDescription> obj = null;

      try
      {
        if(states != null)
        {
          obj = await CollStateDescr
            .Find(s => s.alarm == true && states.Contains(s.state))
            .ToListAsync();
        }
        else
        {
          obj = await CollStateDescr?.Find(s => s.alarm == true).ToListAsync();
        }        
      }
      catch (Exception)
      {

      }
      var retVal = new Dictionary<string, ObjectStateDescriptionDTO>();

      if (obj != null)
      {
        var list = ConvertListStateDescrDB2DTO(obj);

        foreach (var state in list)
        {
          retVal[state.state] = state;
        }
      }      
      return retVal;
    }

    public async Task<Dictionary<string, ObjectStateDTO>> GetAlarmedStates(List<string> statesFilter)
    {     
      var retVal = new Dictionary<string, ObjectStateDTO>();

      var alarmedDescr = await GetAlarmStatesDescr(statesFilter);

      var builder = Builders<DBObjectState>.Filter;
      var filter = builder.Empty;

      foreach (var state in alarmedDescr)
      {
        if (filter == builder.Empty)
        {
          filter = Builders<DBObjectState>.Filter.Where(x => x.states.Contains(state.Value.state));
        }
        else 
        {
          filter = filter | Builders<DBObjectState>.Filter.Where(x => x.states.Contains(state.Value.state));
        }          
      }

      using (var cursor = await _collState.FindAsync(filter, new FindOptions<DBObjectState>() { BatchSize = 1000 }))
      {
        var available =  await cursor.MoveNextAsync();

        while (available)
        {
          var states = cursor.Current.ToList();
          retVal.Union(ConvertListStateDB2DTO(states));
          available = await cursor.MoveNextAsync();
        }
      }

      return retVal;
    }
  }
}
