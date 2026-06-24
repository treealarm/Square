using LeafletAlarmsGrpc;
using SquareIntegrationClient;

namespace IntegrationUtilsLib
{
  /// <summary>
  /// Оркестрация интеграции продьюсера: создание/поиск главного (root) объекта по APP_ID.
  /// Весь транспорт делегируется в <see cref="ISquareIntegration"/>.
  /// </summary>
  public class IntegrationSync
  {
    protected readonly ISquareIntegration _square;

    private ProtoObject? _mainObject = null;
    private IntegroProto? _mainIntegro = null;
    public const string MainStr = "main";
    public ProtoObject? MainObj => _mainObject;
    public IntegroProto? MainIntegroObj => _mainIntegro;

    public IntegrationSync() : this(SquareIntegration.Default) { }
    public IntegrationSync(ISquareIntegration square)
    {
      _square = square;
    }

    public async Task<ProtoObject?> GetBaseObject(string id_in)
    {
      var objs = await _square.GetObjects(new[] { id_in });
      return objs?.FirstOrDefault();
    }

    public Task<List<ProtoObject>?> GetBaseObjects(IEnumerable<string> ids_in)
      => _square.GetObjects(ids_in);

    public Task<List<ProtoObjProps>?> GetPropObjects(IEnumerable<string> ids_in)
      => _square.GetProperties(ids_in);

    public Task<List<IntegroProto>?> GetIntegroObjectsByType(string type)
      => _square.GetIntegroByType(type);

    public Task<List<IntegroProto>?> GetIntegroObjectsByIds(List<string> ids)
      => _square.GetIntegroByIds(ids);

    public async Task<ProtoObject?> UpdateBaseObject(string id, string name, string parent_id)
    {
      var list = new ProtoObjectList();
      list.Objects.Add(new ProtoObject()
      {
        Id = id,
        Name = name,
        ParentId = parent_id
      });
      var objs = await _square.UpsertObjects(list);
      return objs?.FirstOrDefault();
    }

    public Task InitTypes(IntegroTypesProto types, CancellationToken token)
      => _square.RegisterIntegroTypes(types);

    public async Task InitMainObject(CancellationToken token)
    {
      while (_mainObject == null && !token.IsCancellationRequested)
      {
        await Task.Delay(500);
        Console.Error.WriteLine($"creating _mainObject {_square.AppId}");

        // Ищем уже созданный main объект по типу
        var integroObjects = await GetIntegroObjectsByType(MainStr);

        if (integroObjects == null)
        {
          Console.Error.WriteLine("integroObjects is null");
          continue;
        }

        if (integroObjects.Count > 0)
        {
          // Если находим, обращаемся к таблице Objects за базовым объектом (имя, id, ...)
          _mainIntegro = integroObjects.FirstOrDefault();

          if (_mainIntegro != null)
          {
            var mainObj = await GetBaseObject(_mainIntegro.ObjectId);

            if (mainObj == null)
            {
              // Если не находим, создаём с дефолтным именем
              mainObj = await UpdateBaseObject(_mainIntegro.ObjectId, $"{_square.AppId}_{MainStr}", string.Empty);
            }

            if (mainObj != null)
            {
              _mainObject = mainObj;
            }
          }
        }
        else
        {
          // Не нашли объект в БД — создаём новый интеграционный root.
          var mainUid = await _square.GenerateObjectId($"{MainStr}_{_square.AppId}", 0);

          if (!string.IsNullOrEmpty(_mainIntegro?.ObjectId))
          {
            mainUid = _mainIntegro.ObjectId;
          }

          var integro = new IntegroListProto();
          integro.Objects.Add(new IntegroProto()
          {
            IType = MainStr,
            ObjectId = mainUid
          });
          await _square.RegisterIntegro(integro);
        }
      }
    }
  }
}
