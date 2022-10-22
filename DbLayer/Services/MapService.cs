using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ObjectInterfaces;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace DbLayer.Services
{
  public class MapService : IMapService
  {
    private readonly IMongoCollection<DBMarker> _markerCollection;

    private readonly IMongoCollection<DBMarkerProperties> _propCollection;

    private readonly MongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDB;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDBSettings;

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _geoStoreDBSettings = geoStoreDatabaseSettings;
      _mongoClient = new MongoClient(
          geoStoreDatabaseSettings.Value.ConnectionString);

      _mongoDB = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _markerCollection = _mongoDB.GetCollection<DBMarker>(
          geoStoreDatabaseSettings.Value.ObjectsCollectionName);

      _propCollection = _mongoDB.GetCollection<DBMarkerProperties>(
          geoStoreDatabaseSettings.Value.PropCollectionName);

    }

    public async Task<List<BaseMarkerDTO>> GetAsync(List<string> ids)
    {
      var list = await _markerCollection.Find(i => ids.Contains(i.id)).ToListAsync();
      return ConvertMarkerListDB2DTO(list);
    }

    public async Task<BaseMarkerDTO> GetAsync(string id)
    {
      var result = await _markerCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return ConvertMarkerDB2DTO(result);
    }


    public async Task<List<BaseMarkerDTO>> GetByParentIdAsync(
      string parent_id,
      string start_id,
      string end_id,
      int count
    )
    {
      List<DBMarker> retVal;

      if (start_id != null)
      {
        var filter = Builders<DBMarker>.Filter.Gte("_id", new ObjectId(start_id))
          & Builders<DBMarker>.Filter.Eq("parent_id", parent_id);

        retVal = await _markerCollection
          .Find(filter)
          .Limit(count)
          .ToListAsync();
      }
      else if (end_id != null)
      {
        var filter = Builders<DBMarker>.Filter.Lte("_id", new ObjectId(end_id))
          & Builders<DBMarker>.Filter.Eq("parent_id", parent_id);

        retVal = await _markerCollection
          .Find(filter)
          .SortByDescending(x => x.id)
          .Limit(count)
          .ToListAsync()
          ;

        retVal.Sort((x, y) => new ObjectId(x.id).CompareTo(new ObjectId(y.id)));
      }
      else
      {
        retVal = await _markerCollection
                .Find(x => x.parent_id == parent_id)
                .Limit(count)
                .ToListAsync();
      }
      
      return ConvertMarkerListDB2DTO(retVal);
    }

    public async Task<List<BaseMarkerDTO>> GetByNameAsync(string name)
    {
      var retVal = await _markerCollection.Find(x => x.name == name).ToListAsync();
      return ConvertMarkerListDB2DTO(retVal);
    }

    public async Task<List<BaseMarkerDTO>> GetByChildIdAsync(string object_id)
    {
      var parents = new List<BaseMarkerDTO>();
      var marker = await GetAsync(object_id);      

      while (marker != null)
      {
        if (string.IsNullOrEmpty(marker.id))
        {
          // Normally id == null is impossible.
          break;
        }

        parents.Add(marker);

        var dbMarker = await _markerCollection
          .Find(x => x.id == marker.parent_id)
          .FirstOrDefaultAsync();

        marker = ConvertMarkerDB2DTO(dbMarker);
      }
      return parents;
    }

    public async Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id)
    {
      var result = new List<BaseMarkerDTO>();
      var cb = new ConcurrentBag<List<BaseMarkerDTO>>();

      var children = await GetByParentIdAsync(parent_id, null, null, int.MaxValue);

      cb.Add(children);

      foreach (var item in children)
      {
        var sub_children = await GetAllChildren(item.id);
        cb.Add(sub_children);
      }

      foreach (var list in cb)
      {
        result.AddRange(list);
      }

      return result;
    }

    BaseMarkerDTO ConvertMarkerDB2DTO(DBMarker dbMarker)
    {
      if (dbMarker == null)
      {
        return null;
      }

      BaseMarkerDTO result = new BaseMarkerDTO();
      dbMarker.CopyAllTo(result);
      return result;
    }
    List<BaseMarkerDTO> ConvertMarkerListDB2DTO(List<DBMarker> dbMarkers)
    {
      List<BaseMarkerDTO> result = new List<BaseMarkerDTO>();

      foreach (var dbItem in dbMarkers)
      {
        result.Add(ConvertMarkerDB2DTO(dbItem));
      }

      return result;
    }

    public async Task<List<BaseMarkerDTO>> GetTopChildren(List<string> parentIds)
    {
      var result = await _markerCollection
        .Aggregate()
        .Match(x => parentIds.Contains(x.parent_id))
        //.Group("{ _id : '$parent_id'}")
        .Group(
          z => z.parent_id,
          g => new DBMarker() { id = g.Key })
        .ToListAsync();

      return ConvertMarkerListDB2DTO(result);
    }

    public async Task UpdateHierarchyAsync(IEnumerable<BaseMarkerDTO> updatedList)
    {
      if (!updatedList.Any())
      {
        return;
      }

      var dbUpdated = new Dictionary<BaseMarkerDTO, DBMarker>();
      var bulkWrites = new List<WriteModel<DBMarker>>();

      foreach (var item in updatedList)
      {
        var dbObj = new DBMarker();
        item.CopyAllTo(dbObj);
        dbUpdated.Add(item, dbObj);
        
        var filter = Builders<DBMarker>.Filter.Eq(x => x.id, dbObj.id);

        if (string.IsNullOrEmpty(dbObj.id))
        {
          var request = new InsertOneModel<DBMarker>(dbObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBMarker>(filter, dbObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        }
      }
      
      var writeResult = await _markerCollection.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
      }      
    }

    public async Task<long> RemoveAsync(List<string> ids)
    {
      List<ObjectId> objIds = ids.Select(s => new ObjectId(s)).ToList();

      var filter = Builders<BsonDocument>.Filter.In("meta.figure._id", objIds);

      var tracks = _mongoDB.GetCollection<BsonDocument>(
          _geoStoreDBSettings.Value.TracksCollectionName);

      var res1 = await tracks.DeleteManyAsync(filter);

      var routs = _mongoDB.GetCollection<BsonDocument>(
          _geoStoreDBSettings.Value.RoutsCollectionName);

      await routs.DeleteManyAsync(filter);

      var filter1 = Builders<BsonDocument>.Filter.In("_id", objIds);

      var states = _mongoDB.GetCollection<BsonDocument>(
      _geoStoreDBSettings.Value.StateCollectionName);

      await states.DeleteManyAsync(filter1);

      await _propCollection.DeleteManyAsync(x => ids.Contains(x.id));
      var result = await _markerCollection.DeleteManyAsync(x => ids.Contains(x.id));

      return result.DeletedCount;
    }

    private static List<DBObjExtraProperty> ConvertExtraPropsToDB(List<ObjExtraPropertyDTO> extra_props)
    {
      var ep_db = new List<DBObjExtraProperty>();
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

        DBObjExtraProperty newProp = new DBObjExtraProperty()
        {
          prop_name = prop.prop_name,
          visual_type = prop.visual_type
        };

        if (prop.visual_type == BsonType.DateTime.ToString())
        {
          newProp.MetaValue = new BsonDocument(
            "str_val",
            DateTime
              .Parse(prop.str_val)
              .ToUniversalTime()
            );
        }
        else
        {
          newProp.MetaValue = new BsonDocument(
            "str_val",
            prop.str_val
            );
        }
        ep_db.Add(newProp);
      }
      return ep_db;
    }
    private static DBMarkerProperties ConvertDTO2Property(BaseMarkerDTO propsIn)
    {
      var props = propsIn as IObjectProps;

      DBMarkerProperties mProps = new DBMarkerProperties()
      {
        extra_props = new List<DBObjExtraProperty>(),
        id = propsIn.id
      };

      if (props.extra_props == null)
      {
        return mProps;
      }

      mProps.extra_props = ConvertExtraPropsToDB(props.extra_props);

      return mProps;
    }

    public async Task UpdatePropAsync(ObjPropsDTO updatedObj)
    {
      var props = ConvertDTO2Property(updatedObj);

      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;
      await _propCollection.ReplaceOneAsync(x => x.id == updatedObj.id, props, opt);
    }

    public async Task UpdatePropNotDeleteAsync(IEnumerable<BaseMarkerDTO> listUpdate)
    {
      if (!listUpdate.Any())
      {
        return;
      }

      var bulkWrites = new List<WriteModel<DBMarkerProperties>>();

      foreach (var updatedObj in listUpdate)
      {
        var props = updatedObj as IObjectProps;

        if (props?.extra_props == null || props.extra_props.Count == 0)
        {
          continue;
        }        

        DBMarkerProperties propToUpdate = ConvertDTO2Property(updatedObj);

        var propNames = propToUpdate.extra_props.Select(p => p.prop_name).ToList();

        var updatePull = Builders<DBMarkerProperties>.Update
                .PullFilter(u => u.extra_props, c => propNames.Contains(c.prop_name));

        var updatePush = Builders<DBMarkerProperties>.Update
                .PushEach(u => u.extra_props, propToUpdate.extra_props);


        var filter = Builders<DBMarkerProperties>.Filter
          .Where(x => x.id == updatedObj.id);

        var request = new UpdateOneModel<DBMarkerProperties>(filter, updatePull);
        request.IsUpsert = true;
        bulkWrites.Add(request);
        request = new UpdateOneModel<DBMarkerProperties>(filter, updatePush);
        request.IsUpsert = true;
        bulkWrites.Add(request);
      }

      if (bulkWrites.Count == 0)
      {
        return;
      }

      var writeResult = await _propCollection.BulkWriteAsync(bulkWrites);

      //await _propCollection.UpdateOneAsync(filter, updatePull);
      //await _propCollection.UpdateOneAsync(filter, updatePush);
    }

    public async Task<ObjPropsDTO> GetPropAsync(string id)
    {
      var obj = await _propCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return Conver2Property2DTO(obj);
    }

    public async Task<List<ObjPropsDTO>> GetPropsAsync(List<string> ids)
    {
      List<ObjPropsDTO> list = new List<ObjPropsDTO>();
      var objs = await _propCollection.Find(x => ids.Contains(x.id)).ToListAsync();

      foreach (var obj in objs)
      {
        list.Add(Conver2Property2DTO(obj));
      }
      return list;
    }

    public async Task<List<ObjPropsDTO>> GetPropByValuesAsync(
      ObjPropsSearchDTO propFilter,
      string start_id,
      bool forward,
      int count
    )
    {
      List<ObjPropsDTO> ret = new List<ObjPropsDTO>();
      List<DBMarkerProperties> retObjProps = new List<DBMarkerProperties>();

      var builder = Builders<DBMarkerProperties>.Filter;
      var filter = builder.Empty;

      var filterPaging = builder.Empty;

      if (!string.IsNullOrEmpty(start_id))
      {
        if (forward)
          filterPaging = Builders<DBMarkerProperties>.Filter.Gt("_id", new ObjectId(start_id));
        else
          filterPaging = Builders<DBMarkerProperties>.Filter.Lt("_id", new ObjectId(start_id));
      }

      foreach (var prop in propFilter.props)
      {
        var request =
          string.Format("{{prop_name:'{0}', str_val:'{1}'}}",
          prop.prop_name,
          prop.str_val);

        var f1 = Builders<DBMarkerProperties>
          .Filter
          .ElemMatch(t => t.extra_props, request)
          ;

        var metaValue = new BsonDocument(
            "str_val",
            prop.str_val
            );

        if (filter == builder.Empty)
        {
          filter = f1;
        }
        else
        {
          filter |= f1;
        }
      }

      if (filterPaging != builder.Empty)
      {
        filter = filter & filterPaging;
      }


      if (forward)
      {
        retObjProps = await _propCollection
        .Find(filter)
        .Limit(count)
        .ToListAsync();
      }
      else
      {
        retObjProps = await _propCollection
                  .Find(filter)
                  .SortByDescending(x => x.id)
                  .Limit(count)
                  .ToListAsync()
                  ;

        retObjProps.Sort((x, y) => new ObjectId(x.id).CompareTo(new ObjectId(y.id)));
      }
     

      foreach (var obj in retObjProps)
      {
        ret.Add(Conver2Property2DTO(obj));
      }

      return ret;
    }

    public static ObjPropsDTO Conver2Property2DTO(DBMarkerProperties props)
    {
      if (props == null)
      {
        return null;
      }

      ObjPropsDTO mProps = new ObjPropsDTO()
      {
        extra_props = new List<ObjExtraPropertyDTO>(),
        id = props.id
      };

      foreach (var prop in props.extra_props)
      {
        ObjExtraPropertyDTO newProp = new ObjExtraPropertyDTO()
        {
          prop_name = prop.prop_name,
          str_val = prop.MetaValue.GetValue("str_val", string.Empty).ToString(),
          visual_type = prop.visual_type
        };
        mProps.extra_props.Add(newProp);
      }

      return mProps;
    }
  }
}
