using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
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
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer
{
  public class MapService: IMapService
  {
    private readonly IMongoCollection<DBMarker> _markerCollection;

    private readonly IMongoCollection<DBMarkerProperties> _propCollection;

    private readonly MongoClient _mongoClient;

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
          geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _markerCollection = mongoDatabase.GetCollection<DBMarker>(
          geoStoreDatabaseSettings.Value.ObjectsCollectionName);

      _propCollection = mongoDatabase.GetCollection<DBMarkerProperties>(
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
        

    public async Task<List<BaseMarkerDTO>> GetByParentIdAsync(string parent_id)
    {
      var retVal =  await _markerCollection.Find(x => x.parent_id == parent_id).ToListAsync();
      return ConvertMarkerListDB2DTO(retVal);
    }

    public async Task<List<BaseMarkerDTO>> GetByChildIdAsync(string object_id)
    {
      var parents = new List<BaseMarkerDTO>();
      var marker = await GetAsync(object_id);

      while (marker != null)
      {
        parents.Add(marker);
        var dbMarker = await _markerCollection.Find(x => x.id == marker.parent_id).FirstOrDefaultAsync();
        marker = ConvertMarkerDB2DTO(dbMarker);
      }
      return parents;
    }

    public async Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id)
    {
      var result = new List<BaseMarkerDTO>();
      var cb = new ConcurrentBag<List<BaseMarkerDTO>>();

      var children = await GetByParentIdAsync(parent_id);
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

    public async Task CreateAsync(BaseMarkerDTO newObj)
    {
      var dbObj = new DBMarker();
      newObj.CopyAllTo(dbObj);
      await _markerCollection.InsertOneAsync(dbObj);
    }

    public async Task UpdateAsync(BaseMarkerDTO updatedObj)
    {
      var dbObj = new DBMarker();
      updatedObj.CopyAllTo(dbObj);

      await _markerCollection.ReplaceOneAsync(x => x.id == dbObj.id, dbObj);
    }

    public async Task<long> RemoveAsync(List<string> ids)
    {
      
      await _propCollection.DeleteManyAsync(x => ids.Contains(x.id));
      var result =  await _markerCollection.DeleteManyAsync(x => ids.Contains(x.id));
      return result.DeletedCount;
    }

    public async Task<FigureBaseDTO> CreateCompleteObject(FigureBaseDTO figure)
    { 
      var marker = new BaseMarkerDTO();
      marker.name = figure.name;
      marker.parent_id = figure.parent_id;

      if (!string.IsNullOrEmpty(figure.id))
      {
        marker.id = figure.id;
        await UpdateAsync(marker);
      }
      else
      {
        await CreateAsync(marker);
      }

      figure.id = marker.id; 

      return figure;
    }

    public static DBMarkerProperties ConvertDTO2Property(ObjPropsDTO props)
    {
      DBMarkerProperties mProps = new DBMarkerProperties()
      {
        extra_props = new List<DBObjExtraProperty>(),
        id = props.id
      };

      if (props.extra_props == null)
      {
        return mProps;
      }

      var propertieNames = typeof(FigureZoomedDTO).GetProperties().Select(x => x.Name).ToList();

      propertieNames.AddRange(
        typeof(FigureCircleDTO).GetProperties().Select(x => x.Name)
        );


      foreach (var prop in props.extra_props)
      {
        // "radius", "min_zoom", "max_zoom"
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
            DateTime.Parse(prop.str_val)
            );
        }
        else
        {
          newProp.MetaValue = new BsonDocument(
            "str_val",
            prop.str_val
            );
        }
        mProps.extra_props.Add(newProp);
      }

      return mProps;
    }

    public async Task UpdatePropAsync(ObjPropsDTO updatedObj)
    {
      var props = ConvertDTO2Property(updatedObj);

      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;
      await _propCollection.ReplaceOneAsync(x => x.id == updatedObj.id, props, opt);
    }

    public async Task<ObjPropsDTO> GetPropAsync(string id)
    {
      var obj = await _propCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return Conver2Property2DTO(obj);
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
