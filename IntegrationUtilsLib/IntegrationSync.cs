using LeafletAlarmsGrpc;

namespace IntegrationUtilsLib
{
  public class IntegrationSync
  {
    private ProtoObject? _mainObject = null;
    public const string MainStr = "main";
    public async Task<ProtoObject?> GetBaseObject(string id_in)
    {
      var client = Utils.Client;

      var ids = new ProtoObjectIds();
      ids.Ids.Add(id_in);
      var response = await client.RequestObjects(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.FirstOrDefault();
    }
    public async Task<List<ProtoObject>?> GetBaseObjects(IEnumerable<string> ids_in)
    {
      var client = Utils.Client;

      var ids = new ProtoObjectIds();
      ids.Ids.AddRange(ids_in);
      var response = await client.RequestObjects(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }

    public async Task<List<IntegroProto>?> GetIntegroObjects(string type)
    {
      var client = Utils.ClientIntegro;

      var integroRequest = new GetListByTypeRequest();
      integroRequest.IName = client.AppId;
      integroRequest.IType = type;

      // Ищем уже созданный main объект по типу
      var response = await client!.Client!.GetListByTypeAsync(integroRequest);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }

    public async Task<ProtoObject?> UpdateBaseObject(string id, string name)
    {
      var client = Utils.Client;

      var mainObject = new ProtoObject()
      {
        Id = id,
        Name = name
      };
      var list = new ProtoObjectList();
      list.Objects.Add(mainObject);
      var response = await client.UpdateObjects(list);

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
      IntegroProto? mainIntegro = null;

      while (_mainObject == null && !token.IsCancellationRequested)
      {
        await Task.Delay(500);
        var client = Utils.ClientIntegro;

        // Ищем уже созданный main объект по типу
        var integroObjects = await GetIntegroObjects(MainStr);

        if (integroObjects != null)
        {
          if (integroObjects.Count > 0)
          {
            //Если находим, то обращаемся к таблице Objects
            // И получаем базовый объект с именем, айди, итд
            mainIntegro = integroObjects.FirstOrDefault();

            if (mainIntegro != null)
            {
              //Request real object or create if doesn't exist
              var mainObj = await GetBaseObject(mainIntegro.ObjectId);

              if (mainObj == null)
              {
                // Если не находим, то создаем с дефолтным именем
                mainObj = await UpdateBaseObject(mainIntegro.ObjectId, $"{client.AppId}_{MainStr}");
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
            var mainUid = await Utils.GenerateObjectId($"{MainStr}_{client}", 0);

            if (!string.IsNullOrEmpty(mainIntegro?.ObjectId))
            {
              mainUid = mainIntegro.ObjectId;
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
      }
    }
  }
}
