using Domain;
using Microsoft.EntityFrameworkCore;
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
    private readonly PgDbContext _db;

    private readonly IMongoCollection<DBMarkerProperties> _propCollection;

    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDB;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDBSettings;   

    public MapService(
        IOptions<MapDatabaseSettings> geoStoreDatabaseSettings, 
        IMongoClient mongoClient, 
        PgDbContext db)
    {
      _geoStoreDBSettings = geoStoreDatabaseSettings;
      _mongoClient = mongoClient;

      _mongoDB = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _db = db;

      _propCollection = _mongoDB.GetCollection<DBMarkerProperties>(
          geoStoreDatabaseSettings.Value.PropCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
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
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g != null)
          .Select(g => g.Value)
          .ToList();

      if (guids.Count == 0)
        return new Dictionary<string, BaseMarkerDTO>();

      var entities = await _db.Markers
          .Where(m => guids.Contains(m.id))
          .ToListAsync();

      var result = new Dictionary<string, BaseMarkerDTO>();

      foreach (var entity in entities)
      {
        var dto = new MarkerDTO();
        entity.CopyAllTo(dto);

        // для совместимости возвращаем ключ словаря как ObjectId-строку
        var key = Domain.Utils.ConvertGuidToObjectId(entity.id);
        result[key] = dto;
      }

      return result;
    }


    public async Task<BaseMarkerDTO> GetAsync(string id)
    {
      var guid = Domain.Utils.ConvertObjectIdToGuid(id);
      if (guid == null)
        return null;

      var entity = await _db.Markers.FirstOrDefaultAsync(m => m.id == guid.Value);
      if (entity == null)
        return null;

      var dto = new MarkerDTO();
      entity.CopyAllTo(dto); // твой extension для копирования
      return dto;
    }

    public async Task<BaseMarkerDTO> GetParent(string id)
    {
      var guid = Domain.Utils.ConvertObjectIdToGuid(id);
      if (guid == null)
        return null;

      var entity = await _db.Markers
          .Where(m => m.id == guid.Value)
          .Select(m => m.parent_id)
          .FirstOrDefaultAsync();

      if (entity == Guid.Empty)
        return null;

      var parent = await _db.Markers.FirstOrDefaultAsync(m => m.id == entity);
      if (parent == null)
        return null;

      var dto = new MarkerDTO();
      parent.CopyAllTo(dto);
      return dto;
    }


    public async Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdsAsync(
        List<string> parent_ids,
        string start_id,
        string end_id,
        int count)
    {
      // Сначала конвертируем parent_ids в Guid, игнорируя пустые строки
      var parentGuids = parent_ids
          .Where(id => !string.IsNullOrEmpty(id))
          .Select(id => Domain.Utils.ConvertObjectIdToGuid(id))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      // Флаг, есть ли среди parent_ids пустые или null
      bool includeNull = parent_ids.Any(string.IsNullOrEmpty);


      if (!parentGuids.Any() && !includeNull)
        return new Dictionary<string, BaseMarkerDTO>();

      IQueryable<DBMarker> query = _db.Markers
          .Where(m => (m.parent_id.HasValue && parentGuids.Contains(m.parent_id.Value))
                      || (!m.parent_id.HasValue && includeNull))
          .OrderBy(m => m.id);


      // Применяем фильтр по start_id / end_id
      if (!string.IsNullOrEmpty(start_id))
      {
        var startGuid = Domain.Utils.ConvertObjectIdToGuid(start_id);
        if (startGuid != null)
        {
          query = query.Where(m => m.id.CompareTo(startGuid.Value) >= 0);            
        }
      }
      else if (!string.IsNullOrEmpty(end_id))
      {
        var endGuid = Domain.Utils.ConvertObjectIdToGuid(end_id);
        if (endGuid != null)
        {
          query = query.Where(m => m.id.CompareTo(endGuid.Value) <= 0)
                       .OrderByDescending(m => m.id);
        }
      }

      var list = await query.Take(count).ToListAsync();

      // Если сортировали по end_id, то вернём по возрастанию
      if (!string.IsNullOrEmpty(end_id))
        list.Sort((x, y) => x.id.CompareTo(y.id));

      return list.ToDictionary(m => Domain.Utils.ConvertGuidToObjectId(m.id), m =>
      {
        var dto = new BaseMarkerDTO();
        m.CopyAllTo(dto);
        return dto;
      });
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
      var list = await _db.Markers
          .Where(m => m.name == name)
          .ToListAsync();

      return list.ToDictionary(
          m => Domain.Utils.ConvertGuidToObjectId(m.id), // ключ — старый ObjectId строкой
          m =>
          {
            var dto = new BaseMarkerDTO();
            m.CopyAllTo(dto);
            return dto;
          });
    }


    public async Task<List<BaseMarkerDTO>> GetByChildIdAsync(string object_id)
    {
      var parents = new List<BaseMarkerDTO>();

      var marker = await GetAsync(object_id); // используем уже адаптированный GetAsync(string id)
      while (marker != null)
      {
        parents.Add(marker);

        if (string.IsNullOrEmpty(marker.parent_id))
        {
          break; // достигли корня
        }

        var parentGuid = Domain.Utils.ConvertObjectIdToGuid(marker.parent_id);
        if (parentGuid == null)
          break;

        var parentEntity = await _db.Markers
            .FirstOrDefaultAsync(m => m.id == parentGuid.Value);

        if (parentEntity == null)
          break;

        marker = new BaseMarkerDTO
        {
          parent_id = parentEntity.parent_id.HasValue ? Domain.Utils.ConvertGuidToObjectId(parentEntity.parent_id.Value) : null,
          owner_id = parentEntity.owner_id.HasValue ? Domain.Utils.ConvertGuidToObjectId(parentEntity.owner_id.Value) : null,
          name = parentEntity.name,
          id = Domain.Utils.ConvertGuidToObjectId(parentEntity.id)
        };
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
        result.Add(Domain.Utils.ConvertGuidToObjectId(dbItem.id), ConvertMarkerDB2DTO(dbItem));
      }

      return result;
    }

    public async Task<Dictionary<string, BaseMarkerDTO>> GetTopChildren(List<string> parentIds)
    {
      // Конвертируем parentIds в Guid, игнорируя пустые или некорректные
      var parentGuids = parentIds
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!parentGuids.Any())
        return new Dictionary<string, BaseMarkerDTO>();

      // Берём "первого" ребёнка для каждого parent_id
      var topChildren = await _db.Markers
          .Where(m => m.parent_id.HasValue && parentGuids.Contains(m.parent_id.Value))
          .GroupBy(m => m.parent_id.Value)
          .Select(g => g.OrderBy(m => m.id).FirstOrDefault())
          .ToListAsync();

      var dict = new Dictionary<string, BaseMarkerDTO>();
      foreach (var entity in topChildren)
      {
        if (entity != null)
        {
          var dto = new BaseMarkerDTO
          {
            id = Domain.Utils.ConvertGuidToObjectId(entity.id),
          };
          dict[Domain.Utils.ConvertGuidToObjectId(entity.parent_id??Guid.Empty)] = dto;
        }
      }

      return dict;
    }



    public async Task UpdateHierarchyAsync(IEnumerable<BaseMarkerDTO> updatedList)
    {
      if (!updatedList.Any())
        return;

      var ids = updatedList
        .Select(u => Domain.Utils.ConvertObjectIdToGuid(u.id))
        .Where(g => g.HasValue)
        .Select(g => g.Value)
        .ToList();

      var existingEntities = await _db.Markers
          .Where(m => ids.Contains(m.id))
          .ToDictionaryAsync(m => m.id);

      var dbUpdated = new Dictionary<BaseMarkerDTO, DBMarker>();

      foreach (var item in updatedList)
      {
        var dbObj = new DBMarker();
        item.CopyAllTo(dbObj);

        // parent_id и owner_id как Guid? (nullable)
        dbObj.parent_id = string.IsNullOrEmpty(item.parent_id)
            ? null
            : Domain.Utils.ConvertObjectIdToGuid(item.parent_id);

        dbObj.owner_id = string.IsNullOrEmpty(item.owner_id)
            ? null
            : Domain.Utils.ConvertObjectIdToGuid(item.owner_id);

        // id
        if (string.IsNullOrEmpty(item.id))
        {
          var newGuid = Guid.NewGuid();
          dbObj.id = Domain.Utils.ConvertObjectIdToGuid(
              Domain.Utils.ConvertGuidToObjectId(newGuid)) ?? newGuid;
        }

        dbUpdated.Add(item, dbObj);
      }

      // Сохраняем все объекты через EF Core
      foreach (var kvp in dbUpdated)
      {
        if (!existingEntities.TryGetValue(kvp.Value.id, out var entity))
        {
          _db.Markers.Add(kvp.Value);
        }
        else
        {
          _db.Markers.Update(entity);
        }          
      }

      await _db.SaveChangesAsync();

      // Обновляем DTO
      foreach (var pair in dbUpdated)
      {
        pair.Key.id = Domain.Utils.ConvertGuidToObjectId(pair.Value.id);
      }
    }


    public async Task<long> RemoveAsync(List<string> ids)
    {
      List<ObjectId> objIds = ids.Select(s => new ObjectId(s)).ToList();
      // Конвертируем ObjectId строки в Guid
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!guids.Any())
        return 0;

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

      var deletedCount = await _db.Markers
        .Where(m => guids.Contains(m.id))
        .ExecuteDeleteAsync();

      return deletedCount;
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

    public async Task UpdatePropAsync(IEnumerable<ObjPropsDTO> updatedObjs)
    {
      var models = new List<WriteModel<DBMarkerProperties>>();

      foreach (var dto in updatedObjs)
      {
        var props = ConvertDTO2Property(dto);

        var filter = Builders<DBMarkerProperties>.Filter.Eq(x => x.id, dto.id);
        var replace = new ReplaceOneModel<DBMarkerProperties>(filter, props)
        {
          IsUpsert = true
        };

        models.Add(replace);
      }

      if (models.Count > 0)
      {
        await _propCollection.BulkWriteAsync(models);
      }
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
      // Конвертируем ObjectId строки в Guid
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!guids.Any())
        return new Dictionary<string, BaseMarkerDTO>();

      // Получаем все объекты с этими id
      var listObjects = await _db.Markers
          .Where(m => guids.Contains(m.id))
          .ToListAsync();

      // Представления — объекты с owner_id != null
      var views = listObjects
          .Where(m => m.owner_id.HasValue)
          .ToList();

      if (!views.Any())
      {
        // Все объекты без owner_id
        return ConvertMarkerListDB2DTO(listObjects);
      }

      // Владельцы — объекты с owner_id == null
      var owners = listObjects
          .Where(m => !m.owner_id.HasValue)
          .ToList();

      var ownerGuids = views
          .Select(v => v.owner_id.Value)
          .ToList();

      // Получаем владельцев из базы, которых еще нет в listObjects
      var ownersDb = await _db.Markers
          .Where(m => !m.owner_id.HasValue && ownerGuids.Contains(m.id))
          .ToListAsync();

      owners = owners.Union(ownersDb).ToList();

      return ConvertMarkerListDB2DTO(owners);
    }


    public async Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAndViewsAsync(List<string> ids)
    {
      // Конвертируем строки в Guid
      var guids = ids
          .Select(Domain.Utils.ConvertObjectIdToGuid)
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (!guids.Any())
        return new Dictionary<string, BaseMarkerDTO>();

      // Получаем все маркеры с указанными ID
      var listObjects = await _db.Markers
          .Where(m => guids.Contains(m.id))
          .ToListAsync();

      // Представления — объекты с owner_id != null
      var views = listObjects
          .Where(m => m.owner_id.HasValue)
          .ToList();

      if (!views.Any())
      {
        return ConvertMarkerListDB2DTO(listObjects);
      }

      // Владельцы среди уже полученных объектов (owner_id == null)
      var owners = listObjects
          .Where(m => !m.owner_id.HasValue)
          .ToList();

      // Список owner_id для поиска недостающих владельцев
      var ownerGuids = views
          .Select(v => v.owner_id.Value)
          .ToList();

      // Получаем владельцев из базы, которых ещё нет в owners
      var ownersDb = ownerGuids.Any()
          ? await _db.Markers
              .Where(m => !m.owner_id.HasValue && ownerGuids.Contains(m.id))
              .ToListAsync()
          : new List<DBMarker>();

      // Объединяем представления и владельцев
      var allObjects = views
          .Union(ownersDb)
          .ToList();

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
