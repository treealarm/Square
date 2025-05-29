using Common;
using LeafletAlarmsGrpc;

namespace IntegrationUtilsLib
{
  public class IntegrationSync
  {
    private ProtoObject? _mainObject = null;
    private IntegroProto? _mainIntegro = null;
    public const string MainStr = "main";
    public ProtoObject? MainObj { get { return _mainObject; } }
    public IntegroProto? MainIntegroObj { get { return _mainIntegro; } }
    public async Task<ProtoObject?> GetBaseObject(string id_in)
    {
      var client = Utils.ClientBase.Client;

      var ids = new ProtoObjectIds();
      ids.Ids.Add(id_in);
      var response = await client!.RequestObjectsAsync(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.FirstOrDefault();
    }
    public async Task<List<ProtoObject>?> GetBaseObjects(IEnumerable<string> ids_in)
    {
      var client = Utils.ClientBase.Client;

      var ids = new ProtoObjectIds();
      ids.Ids.AddRange(ids_in);
      var response = await client!.RequestObjectsAsync(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }
    public async Task<List<ProtoObjProps>?> GetPropObjects(IEnumerable<string> ids_in)
    {
      var client = Utils.ClientBase;

      var ids = new ProtoObjectIds();
      ids.Ids.AddRange(ids_in);
      var response = await client!.Client!.RequestPropertiesAsync(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }
    public async Task<List<IntegroProto>?> GetIntegroObjectsByType(string type)
    {
      var client = Utils.ClientIntegro;

      var integroRequest = new GetListByTypeRequest();
      integroRequest.IName = client.AppId;
      integroRequest.IType = type;

      // Ищем уже созданный main объект по типу

      try
      {
        var response = await client!.Client!.GetListByTypeAsync(integroRequest);

        if (response == null)
        {
          return null;
        }
        return response.Objects.ToList();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return null;      
    }

    public async Task<List<IntegroProto>?> GetIntegroObjectsByIds(List<string> ids)
    {
      var client = Utils.ClientIntegro;

      var integroRequest = new ProtoObjectIds();
      integroRequest.Ids.AddRange(ids);

      // Ищем уже созданный main объект по типу
      var response = await client!.Client!.GetListByIdsAsync(integroRequest);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }

    public async Task<ProtoObject?> UpdateBaseObject(string id, string name, string parent_id)
    {
      var client = Utils.ClientBase.Client;

      var mainObject = new ProtoObject()
      {
        Id = id,
        Name = name,
        ParentId = parent_id
      };
      var list = new ProtoObjectList();
      list.Objects.Add(mainObject);
      var response = await client!.UpdateObjectsAsync(list);

      if (response == null)
      {
        return null;
      }
      return response.Objects.FirstOrDefault();
    }
    public async Task<bool> InitTypes(IntegroTypesProto types, CancellationToken token)
    {
      var client = Utils.ClientIntegro;
      foreach (var type in types.Types_)
      {
        type.IName = client.AppId;// Setup my app_id as i-name
      }
      var retVal = await client!.Client!.UpdateIntegroTypesAsync(types);
      return retVal.Value;
    }
    public async Task InitMainObject(CancellationToken token)
    {
      while (_mainObject == null && !token.IsCancellationRequested)
      {
        await Task.Delay(500);
        var client = Utils.ClientIntegro;
        Console.Error.WriteLine($"creating _mainObject {client.AppId}");        

        // Ищем уже созданный main объект по типу
        var integroObjects = await GetIntegroObjectsByType(MainStr);

        if (integroObjects != null)
        {
          if (integroObjects.Count > 0)
          {
            //Если находим, то обращаемся к таблице Objects
            // И получаем базовый объект с именем, айди, итд
            _mainIntegro = integroObjects.FirstOrDefault();

            if (_mainIntegro != null)
            {
              //Request real object or create if doesn't exist
              var mainObj = await GetBaseObject(_mainIntegro.ObjectId);

              if (mainObj == null)
              {
                // Если не находим, то создаем с дефолтным именем
                mainObj = await UpdateBaseObject(_mainIntegro.ObjectId, $"{client.AppId}_{MainStr}", string.Empty);
              }

              if (mainObj != null)
              {
                _mainObject = mainObj;
              }
            }
          }
          else
          {
            //Если не нашли объект в БД, то создадим новый.
            var mainUid = await Utils.GenerateObjectId($"{MainStr}_{client.AppId}", 0);

            if (!string.IsNullOrEmpty(_mainIntegro?.ObjectId))
            {
              mainUid = _mainIntegro.ObjectId;
            }

            var integro = new IntegroListProto();
            integro.Objects.Add(new IntegroProto()
            {
              IType = MainStr,
              IName = client.AppId,
              ObjectId = mainUid
            });
            await client!.Client!.UpdateIntegroAsync(integro);
          }
        }
        else
        {
          Console.Error.WriteLine("integroObjects is null");
        }
      }
    }
  }
}
