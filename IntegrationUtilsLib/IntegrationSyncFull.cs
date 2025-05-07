
using Domain;
using LeafletAlarmsGrpc;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IntegrationUtilsLib
{
  public class IntegrationSyncFull: IntegrationSync,IAsyncDisposable
  {
    private readonly ISubService _sub;
    private string? _topic_update_integros = null;
    private string? _topic_delete_integros = null;
    private readonly CancellationToken _token;
    private ConcurrentDictionary<string, IntegroProto> _integros =
      new ConcurrentDictionary<string, IntegroProto>();

    private ConcurrentDictionary<string, ProtoObjProps?> _obj_props =
      new ConcurrentDictionary<string, ProtoObjProps?>();
    public IReadOnlyDictionary<string, ProtoObjProps?> ObjProps => _obj_props;

    private readonly Dictionary<string, List<ProtoObjExtraProperty>?> _type_to_props;
    public Action? OnConfigurationChanged;

    public IntegrationSyncFull(
      ISubService sub,
      Dictionary<string, List<ProtoObjExtraProperty>?> type_to_props,
      CancellationToken token)
    {
      _sub = sub;
      _token = token;
      _type_to_props = type_to_props;
    }
    public async Task InitAll()
    {
      await InitMainObject(_token);
      
      List<IntegroProto>? listIntegros = null;

      while (listIntegros == null)
      {
        listIntegros = await GetIntegroObjectsByType(string.Empty);

        if (_token.IsCancellationRequested)
        {
          return;
        }
      }

      _integros = new ConcurrentDictionary<string, IntegroProto>(
           listIntegros.ToDictionary(proto => proto.ObjectId)
        );

      List<ProtoObjProps>? listProps = null;

      while(listProps == null)
      {
        listProps = await GetPropObjects(_integros.Keys);

        if (_token.IsCancellationRequested)
        {
          return;
        }
      }

      _obj_props = new ConcurrentDictionary<string, ProtoObjProps?>(
          listProps.ToDictionary(
              proto => proto.Id,
              proto => (ProtoObjProps?)proto
          )
      );

      await SyncPropObjects();

      await UpdateMissingPropertiesFromTemplate();

      _topic_update_integros = $"{Topics.OnUpdateIntegros}_{MainIntegroObj!.IName}";
      await _sub.Subscribe(_topic_update_integros, OnUpdateIntegros);

      _topic_delete_integros = $"{Topics.OnDeleteIntegros}_{MainIntegroObj!.IName}";
      await _sub.Subscribe(_topic_delete_integros, OnDeleteIntegros);
    }

    public async Task InitChildrenTypes(Dictionary<string, IEnumerable<string>> typeHierarchy)
    {
      var types = new IntegroTypesProto();

      foreach (var kvp in typeHierarchy)
      {
        var typeProto = new IntegroTypeProto
        {
          IType = kvp.Key
        };

        foreach (var child in kvp.Value)
        {
          typeProto.Children.Add(new IntegroTypeChildProto
          {
            ChildIType = child
          });
        }

        types.Types_.Add(typeProto);
      }

      await InitTypes(types, _token);
    }

    public async Task SyncPropObjects()
    {
      var missingIntegros = _integros
        .Where(kvp => !_obj_props.ContainsKey(kvp.Key))
        .Select(kvp => kvp.Value)
        .ToList();
      var toSend = new ProtoObjPropsList();

      foreach (var integro in missingIntegros)
      {
        var protoProp = new ProtoObjProps()
        {
          Id = integro.ObjectId
        };

        if (_type_to_props.TryGetValue(integro.IType, out var common_props) && common_props != null)
        {
          var clonedProps = common_props
              .Select(p => p.Clone())
              .ToList();

          protoProp.Properties.Add(clonedProps);
          _obj_props[integro.ObjectId] = protoProp;
        }
        else
        {
          _obj_props[integro.ObjectId] = null;
        }
        toSend.Objects.Add(protoProp);
      }
      var client = Utils.ClientBase;
      await client!.Client!.UpdatePropertiesAsync(toSend);
    }
    public async Task OnDeleteIntegros(string channel, byte[] message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null || ids.Count == 0)
      {
        return;
      }
      foreach (var id in ids)
      {
        if (_integros.TryRemove(id, out var removed))
        {
          Console.WriteLine($"Deleted: {removed}");
        }
        else
        {
          Console.WriteLine($"no key {id} while delete");
        }

        if (_obj_props.TryRemove(id, out var removed_p))
        {
          Console.WriteLine($"Deleted: {removed}");
        }
        else
        {
          Console.WriteLine($"no key {id} while delete");
        }
      }

      await SyncPropObjects();
      OnConfigurationChanged?.Invoke();
    }

    public async Task OnUpdateIntegros(string channel, byte[] message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null || ids.Count == 0)
      {
        return;
      }
      var updatedIntegros = await GetIntegroObjectsByIds(ids);

      if (updatedIntegros != null)
      {
        foreach (var obj in updatedIntegros)
        {
          _integros[obj.ObjectId] = obj;
        }
      }      

      var listProps = await GetPropObjects(ids);

      if (listProps != null)
      {
        foreach (var p in listProps)
        {
          _obj_props[p.Id] = p;
        }
      }     

      await SyncPropObjects();

      OnConfigurationChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
      if (!string.IsNullOrEmpty(_topic_update_integros))
      {
        await _sub.Unsubscribe(_topic_update_integros, OnUpdateIntegros);
      }
      if (!string.IsNullOrEmpty(_topic_delete_integros))
      {
        await _sub.Unsubscribe(_topic_delete_integros, OnDeleteIntegros);
      }
    }

    public async Task UpdateMissingPropertiesFromTemplate()
    {
      var toUpdateList = new ProtoObjPropsList();

      foreach (var kvp in _obj_props)
      {
        var objId = kvp.Key;
        var currentProps = kvp.Value;

        if (!_integros.TryGetValue(objId, out var integro))
          continue;

        if (!_type_to_props.TryGetValue(integro.IType, out var templateProps) || templateProps == null)
          continue;

        if (currentProps == null)
        {
          // Объект совсем без свойств — просто копируем шаблон целиком
          var newProps = new ProtoObjProps
          {
            Id = objId,
          };
          newProps.Properties.Add(templateProps);
          _obj_props[objId] = newProps;
          toUpdateList.Objects.Add(newProps);
          continue;
        }

        var existingProps = currentProps.Properties.ToDictionary(p => p.PropName);

        bool changed = false;

        foreach (var templateProp in templateProps)
        {
          if (existingProps.TryGetValue(templateProp.PropName, out var currentProp))
          {
            // Проперти есть, но VisualType может отличаться
            if (currentProp.VisualType != templateProp.VisualType)
            {
              currentProp.VisualType = templateProp.VisualType;
              changed = true;
            }
          }
          else
          {
            // Нет такого свойства — добавим из шаблона
            var newProp = templateProp.Clone();
            currentProps.Properties.Add(newProp);
            changed = true;
          }
        }

        if (changed)
        {
          toUpdateList.Objects.Add(currentProps);
        }
      }

      if (toUpdateList.Objects.Count > 0)
      {
        var client = Utils.ClientBase;
        await client!.Client!.UpdatePropertiesAsync(toUpdateList);
      }
    }
  }
}
