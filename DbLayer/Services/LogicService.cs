using DbLayer.Models;
using Domain;
using Domain.ObjectInterfaces;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class LogicService: ILogicService
  {
    private readonly IMongoCollection<DBStaticLogic> _coll;
    private readonly MongoClient _mongoClient;
    public LogicService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBStaticLogic>
        (geoStoreDatabaseSettings.Value.LogicCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBStaticLogic> keys =
                new IndexKeysDefinitionBuilder<DBStaticLogic>()
                .Ascending(d => d.figs)
                ;

        var indexModel = new CreateIndexModel<DBStaticLogic>(
          keys, new CreateIndexOptions()
          { Name = "figs" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
    }

    private DBStaticLogic ConvertDTO2DB(StaticLogicDTO obj2UpdateIn)
    {
      DBStaticLogic ret = new DBStaticLogic()
      {
        id = obj2UpdateIn.id,
        logic = obj2UpdateIn.logic,
        name = obj2UpdateIn.name
      };

      if (obj2UpdateIn.figs != null)
      {
        List<DBLogicFigureLink> figs;
        obj2UpdateIn.figs.CopyAllToAsJson(out figs);
        ret.figs = figs;
      }

      if (
        obj2UpdateIn.property_filter != null &&
        obj2UpdateIn.property_filter.props != null &&
        obj2UpdateIn.property_filter.props.Count > 0
      )
      {
        ret.property_filter = new DBObjPropsSearch()
        {
          props = new List<DBObjExtraProperty>()
        };

        foreach (var prop in obj2UpdateIn.property_filter.props)
        {
          var prop1 = new DBObjExtraProperty()
          {
            prop_name = prop.prop_name,
            str_val = new BsonDocument(
              "str_val",
              prop.str_val
            )
          };
          ret.property_filter.props.Add(prop1);
        }
      }

      return ret;
    }

    private StaticLogicDTO ConvertDB2DTO(DBStaticLogic obj2UpdateIn)
    {
      StaticLogicDTO ret = new StaticLogicDTO()
      {
        id = obj2UpdateIn.id,
        logic = obj2UpdateIn.logic,
        name = obj2UpdateIn.name
      };

      if (obj2UpdateIn.figs != null)
      {
        List<LogicFigureLinkDTO> figs;
        obj2UpdateIn.figs.CopyAllToAsJson(out figs);
        ret.figs = figs;
      }

      if (obj2UpdateIn.property_filter != null && obj2UpdateIn.property_filter.props != null)
      {
        ret.property_filter = new ObjPropsSearchDTO()
        {
          props = new List<KeyValueDTO>()
        };

        foreach (var prop in obj2UpdateIn.property_filter.props)
        {
          var prop1 = new KeyValueDTO()
          {
            prop_name = prop.prop_name,
            str_val = prop.str_val.GetValue("str_val", string.Empty).ToString()
          };
          ret.property_filter.props.Add(prop1);
        }
      }

      return ret;
    }

    public async Task UpdateAsync(StaticLogicDTO obj2UpdateIn)
    {
      if (obj2UpdateIn.figs != null)
      {
        obj2UpdateIn.figs.RemoveAll(t => string.IsNullOrEmpty(t.id));
      }

      DBStaticLogic obj2Update = ConvertDTO2DB(obj2UpdateIn);

      if (string.IsNullOrEmpty(obj2Update.id))
      {
        await _coll.InsertOneAsync(obj2Update);
        obj2UpdateIn.id = obj2Update.id;
      }
      else
      {
        var options = new ReplaceOptions() { IsUpsert = true };
        await _coll.ReplaceOneAsync(x => x.id == obj2Update.id, obj2Update, options);
      }        
    }

    public async Task DeleteAsync(string id)
    {
      await _coll.DeleteOneAsync(x => x.id == id);
    }

    public async Task<StaticLogicDTO> GetAsync(string id)
    {
      var result = await _coll.Find(x => x.id == id).FirstOrDefaultAsync();

      if (result == null)
      {
        return null;
      }

      StaticLogicDTO ret = ConvertDB2DTO(result);

      return ret;
    }

    private List<StaticLogicDTO> DBListToDTO(List<DBStaticLogic> result)
    {
      List<StaticLogicDTO> listRet = new List<StaticLogicDTO>();

      if (result == null)
      {
        return null;
      }

      foreach (var f in result)
      {
        StaticLogicDTO ret = ConvertDB2DTO(f);
        listRet.Add(ret);
      }

      return listRet;
    }
    public async Task<List<StaticLogicDTO>> GetByFigureAsync(string id)
    {
      //var filter = Builders<DBStaticLogic>.Filter.Eq("figs._id", new ObjectId(id));

      var filter = Builders<DBStaticLogic>.Filter
        .ElemMatch(x => x.figs, d => d.id == id);

      var result = await _coll
        .Find(filter)
        .ToListAsync();

      var listRet = DBListToDTO(result);
      return listRet;
    }

    public async Task<List<StaticLogicDTO>> GetByName(string name)
    {
      var filter = Builders<DBStaticLogic>.Filter        
        .Where(x => x.name.Contains(name));

      var result = await _coll
        .Find(filter)
        .ToListAsync();

      var listRet = DBListToDTO(result);

      return listRet;
    }

    public async Task<List<StaticLogicDTO>> GetListAsync(
      string start_id,
      bool forward,
      int count
    )
    {
      List<DBStaticLogic> retDbList;

      var builder = Builders<DBStaticLogic>.Filter;

      var filter = builder.Empty;

      if (!string.IsNullOrEmpty(start_id))
      {
        if (forward)
          filter = Builders<DBStaticLogic>.Filter.Gt("_id", new ObjectId(start_id));
        else
          filter = Builders<DBStaticLogic>.Filter.Lt("_id", new ObjectId(start_id));
      }

      if (forward)
      {
        retDbList = await _coll
        .Find(filter)
        .Limit(count)
        .ToListAsync();
      }
      else
      {
        retDbList = await _coll
                  .Find(filter)
                  .SortByDescending(x => x.id)
                  .Limit(count)
                  .ToListAsync()
                  ;

        retDbList.Sort((x, y) => new ObjectId(x.id).CompareTo(new ObjectId(y.id)));
      }

      return DBListToDTO(retDbList);
    }
  }
}
