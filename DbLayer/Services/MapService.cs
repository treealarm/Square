using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class MapService : IMapService
  {
    private readonly IMongoCollection<DBMarker> _markerCollection;

    private readonly IMongoCollection<DBMarkerProperties> _propCollection;

    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDB;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDBSettings;   

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings, IMongoClient mongoClient)
    {
      _geoStoreDBSettings = geoStoreDatabaseSettings;
      _mongoClient = mongoClient;

      _mongoDB = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _markerCollection = _mongoDB.GetCollection<DBMarker>(
          geoStoreDatabaseSettings.Value.ObjectsCollectionName);

      _propCollection = _mongoDB.GetCollection<DBMarkerProperties>(
          geoStoreDatabaseSettings.Value.PropCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBMarker> keys =
          new IndexKeysDefinitionBuilder<DBMarker>()
          .Ascending(d => d.parent_id)
          ;

        var indexModel = new CreateIndexModel<DBMarker>(
          keys, new CreateIndexOptions()
          { Name = "parent" }
        );

        _markerCollection.Indexes.CreateOneAsync(indexModel);
      }

      {
        // Индекс для запросов по комбинации id и owner_id
        IndexKeysDefinition<DBMarker> compositeKeys = Builders<DBMarker>.IndexKeys
            .Ascending(d => d.id)
            .Ascending(d => d.owner_id);

        var compositeIndexModel = new CreateIndexModel<DBMarker>(
            compositeKeys, new CreateIndexOptions
            {
              Name = "owner_id"
            });

        _markerCollection.Indexes.CreateOneAsync(compositeIndexModel);

        // Индекс для запросов только по owner_id
        IndexKeysDefinition<DBMarker> ownerKey = Builders<DBMarker>.IndexKeys
            .Ascending(d => d.owner_id);

        var ownerIndexModel = new CreateIndexModel<DBMarker>(
            ownerKey, new CreateIndexOptions
            {
              Name = "owner"
            });

        _markerCollection.Indexes.CreateOneAsync(ownerIndexModel);
      }

      {
        var keys = Builders<DBMarkerProperties>.IndexKeys.Combine(
          Builders<DBMarkerProperties>.IndexKeys
          .Ascending($"{nameof(DBMarkerProperties.extra_props)}.{nameof(DBObjExtraProperty.prop_name)}"),
          Builders<DBMarkerProperties>.IndexKeys
          .Ascending($"{nameof(DBMarkerProperties.extra_props)}.{nameof(DBObjExtraProperty.str_val)}"));

           var indexModel = new CreateIndexModel<DBMarkerProperties>(
          keys, new CreateIndexOptions()
          { Name = "ep" }
        );

        _propCollection.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetAsync(List<string> ids)
    {
      var list = await _markerCollection.Find(i => ids.Contains(i.id)).ToListAsync();
      return ConvertMarkerListDB2DTO(list);
    }

    public async Task<BaseMarkerDTO> GetAsync(string id)
    {
      var result = await _markerCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return ConvertMarkerDB2DTO(result);
    }

    public async Task<BaseMarkerDTO> GetParent(string id)
    {
      var result = await _markerCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return ConvertMarkerDB2DTO(result);
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdsAsync(
      List<string> parent_ids,
      string start_id,
      string end_id,
      int count
    )
    {
      List<DBMarker> retVal;

      if (start_id != null)
      {
        var filter = Builders<DBMarker>.Filter.Gte("_id", new ObjectId(start_id))
          & Builders<DBMarker>.Filter.In("parent_id", parent_ids);

        retVal = await _markerCollection
          .Find(filter)
          .Limit(count)
          .ToListAsync();
      }
      else if (end_id != null)
      {
        var filter = Builders<DBMarker>.Filter.Lte("_id", new ObjectId(end_id))
          & Builders<DBMarker>.Filter.In("parent_id", parent_ids);

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
                .Find(x => parent_ids.Contains(x.parent_id))
                .Limit(count)
                .ToListAsync();
      }

      return ConvertMarkerListDB2DTO(retVal);
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdAsync(
      string parent_id,
      string start_id,
      string end_id,
      int count
    )
    {
      return await GetByParentIdsAsync(new List<string>() { parent_id }, start_id, end_id, count);
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetByNameAsync(string name)
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

      cb.Add(children.Values.ToList());

      foreach (var item in children)
      {
        var sub_children = await GetAllChildren(item.Value.id);
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
    Dictionary<string, BaseMarkerDTO> ConvertMarkerListDB2DTO(List<DBMarker> dbMarkers)
    {
      var result = new Dictionary<string, BaseMarkerDTO>();

      foreach (var dbItem in dbMarkers)
      {
        result.Add(dbItem.id, ConvertMarkerDB2DTO(dbItem));
      }

      return result;
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetTopChildren(List<string> parentIds)
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

        if (string.IsNullOrEmpty(dbObj.parent_id))
        {
          dbObj.parent_id = null;
        }

        if (string.IsNullOrEmpty(dbObj.id))
        {
          dbObj.id = null;
        }

        if (string.IsNullOrEmpty(dbObj.owner_id))
        {
          dbObj.owner_id = null;
        }

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

      var routes = _mongoDB.GetCollection<BsonDocument>(
          _geoStoreDBSettings.Value.RoutesCollectionName);

      await routes.DeleteManyAsync(filter);

      var filter1 = Builders<BsonDocument>.Filter.In("_id", objIds);

      var states = _mongoDB.GetCollection<BsonDocument>(
      _geoStoreDBSettings.Value.StateCollectionName);

      await states.DeleteManyAsync(filter1);

      await _propCollection.DeleteManyAsync(x => ids.Contains(x.id));
      var result = await _markerCollection.DeleteManyAsync(x => ids.Contains(x.id));

      return result.DeletedCount;
    }

    private static DBMarkerProperties ConvertDTO2Property(IObjectProps propsIn)
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

      mProps.extra_props = ModelGate.ConvertExtraPropsToDB(props.extra_props);

      return mProps;
    }

    public async Task UpdatePropAsync(ObjPropsDTO updatedObj)
    {
      var props = ConvertDTO2Property(updatedObj);

      ReplaceOptions opt = new ReplaceOptions();
      opt.IsUpsert = true;
      await _propCollection.ReplaceOneAsync(x => x.id == updatedObj.id, props, opt);
    }

    public async Task UpdatePropNotDeleteAsync(IEnumerable<IObjectProps> listUpdate)
    {
      if (!listUpdate.Any())
      {
        return;
      }

      var bulkWrites = new List<WriteModel<DBMarkerProperties>>();

      // Метод для добавления запроса Pull и Push
      void AddUpdateRequests(DBMarkerProperties propToUpdate, IEnumerable<string> propNames, string id)
      {
        var filter = Builders<DBMarkerProperties>.Filter.Where(x => x.id == id);

        var updatePull = Builders<DBMarkerProperties>.Update.PullFilter(u => u.extra_props, c => propNames.Contains(c.prop_name));
        var updatePush = Builders<DBMarkerProperties>.Update.PushEach(u => u.extra_props, propToUpdate.extra_props);

        bulkWrites.Add(new UpdateOneModel<DBMarkerProperties>(filter, updatePull) { IsUpsert = true });
        bulkWrites.Add(new UpdateOneModel<DBMarkerProperties>(filter, updatePush) { IsUpsert = true });
      }

      foreach (var props in listUpdate)
      {
        // Проверка, что extra_props не пустой
        if (props?.extra_props?.Any() != true)
        {
          continue;
        }

        // Преобразование объекта
        var propToUpdate = ConvertDTO2Property(props);

        // Получение имен свойств
        var propNames = propToUpdate.extra_props.Select(p => p.prop_name).ToList();

        // Добавление запросов Pull и Push
        AddUpdateRequests(propToUpdate, propNames, props.id);
      }

      if (bulkWrites.Any())
      {
        await _propCollection.BulkWriteAsync(bulkWrites);
      }
    }


    public async Task<ObjPropsDTO> GetPropAsync(string id)
    {
      var obj = await _propCollection.Find(x => x.id == id).FirstOrDefaultAsync();
      return ModelGate.Conver2Property2DTO(obj);
    }

    public async Task<Dictionary<string, ObjPropsDTO>> GetPropsAsync(List<string> ids)
    {
      var list = new Dictionary<string, ObjPropsDTO>();
      var objs = await _propCollection.Find(x => ids.Contains(x.id)).ToListAsync();

      foreach (var obj in objs)
      {
        list.Add(obj.id, ModelGate.Conver2Property2DTO(obj));
      }
      return list;
    }

    public async Task<List<ObjPropsDTO>> GetPropByValuesAsync(
      ObjPropsSearchDTO propFilter,
      string start_id,
      int forward,
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
        if (forward > 0)
          filterPaging = Builders<DBMarkerProperties>.Filter.Gt("_id", new ObjectId(start_id));
        else
          filterPaging = Builders<DBMarkerProperties>.Filter.Lt("_id", new ObjectId(start_id));
      }

      if (propFilter != null)
      {
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
      }

      if (filterPaging != builder.Empty)
      {
        filter = filter & filterPaging;
      }


      if (forward > 0)
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
        ret.Add(ModelGate.Conver2Property2DTO(obj));
      }

      return ret;
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAsync(List<string> ids)
    {
      var list_objects = await _markerCollection
        .Find(
         i =>
          ids.Contains(i.id))
        .ToListAsync();

      var views = list_objects
        .Where(i => !string.IsNullOrEmpty(i.owner_id));

      if (!views.Any())
      {
        return ConvertMarkerListDB2DTO(list_objects);
      }

      var owners = list_objects
        .Where(i => string.IsNullOrEmpty(i.owner_id));

      var owner_ids = views.Select(i => i.owner_id);

      // Search only real objects which doesn't have any owner
      // and as a result return owners of objects
      var owners_db = await _markerCollection
        .Find(
         i => 
         string.IsNullOrEmpty(i.owner_id) && 
          owner_ids.Contains(i.id))
        .ToListAsync();

      owners = owners.Union(owners_db);
      return ConvertMarkerListDB2DTO(owners.ToList());
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAndViewsAsync(List<string> ids)
    {
      // Получаем все маркеры с указанными ID
      var listObjects = await _markerCollection
          .Find(i => ids.Contains(i.id))
          .ToListAsync();

      // Находим представления (маркеры с owner_id)
      var views = listObjects
          .Where(i => !string.IsNullOrEmpty(i.owner_id))
          .ToList();

      if (!views.Any())
      {
        return ConvertMarkerListDB2DTO(listObjects);
      }
      // Находим владельцев (owner_id = null) и собираем owner_ids для следующего поиска
      var owners = listObjects
          .Where(i => string.IsNullOrEmpty(i.owner_id))
          .ToList();

      var ownerIds = views
          .Select(i => i.owner_id)
          .ToList();

      // Запрашиваем владельцев из базы, если есть ownerIds
      var ownersDb = ownerIds.Any()
          ? await _markerCollection
              .Find(i => string.IsNullOrEmpty(i.owner_id) && ownerIds.Contains(i.id))
              .ToListAsync()
          : new List<DBMarker>();

      // Объединяем найденные маркеры (владельцев и представления)
      var allObjects = views
          .Union(ownersDb)
          .ToList();

      // Конвертируем в DTO и возвращаем
      return ConvertMarkerListDB2DTO(allObjects);
    }

    public async Task<FiguresDTO> GetFigures(Dictionary<string, GeoObjectDTO> geo)
    {
      var result = new FiguresDTO();

      if (geo == null)
      {
        return result;
      }

      var ids = geo.Keys.ToList();

      var tree = await GetAsync(ids);

      var props = await GetPropsAsync(ids);

      foreach (var item in tree.Values)
      {
        if (geo.TryGetValue(item.id, out var geoPart))
        {
          FigureZoomedDTO retItem = null;

          var figure = new FigureGeoDTO();
          figure.radius = geoPart.radius;
          figure.geometry = geoPart.location;
          result.figs.Add(figure);
          retItem = figure;

          if (retItem != null)
          {
            retItem.id = item.id;
            retItem.name = item.name;
            retItem.parent_id = item.parent_id;
            retItem.zoom_level = geoPart.zoom_level?.ToString();

            if (props.TryGetValue(retItem.id, out var objProp))
            {
              if (retItem.extra_props != null)
              {
                retItem.extra_props.AddRange(objProp.extra_props);
              }
              else
              {
                retItem.extra_props = objProp.extra_props;
              }
            }
          }

        }
      }
      return result;
    }
  }
}
