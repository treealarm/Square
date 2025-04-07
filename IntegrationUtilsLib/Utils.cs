using GrpcDaprLib;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.IntegroService;

namespace IntegrationUtilsLib
{
  public class Utils
  {
    // Статическое поле для хранения клиента
    private static GrpcUpdater? _client;
    private static GrpcUpdaterClient<IntegroServiceClient>? _clientIntegro;
    private static Dictionary<string,string> _idsCash = new Dictionary<string, string>();
    private static readonly object _lock = new object(); // Для синхронизации

    // Метод для доступа к клиенту
    public static GrpcUpdater Client
    {
      get
      {
        // Проверяем, если клиент не существует или мертв, создаем новый
        if (_client == null || _client.IsDead)
        {
          lock (_lock)
          {
            if (_client == null || _client.IsDead)
            {
              _client = new GrpcUpdater();  // Инициализация нового клиента
            }
          }
        }
        return _client;
      }
    }

    public static GrpcUpdaterClient<IntegroServiceClient> ClientIntegro
    {
      get
      {
        // Проверяем, если клиент не существует или мертв, создаем новый
        if (_clientIntegro == null || _clientIntegro.IsDead)
        {
          lock (_lock)
          {
            if (_client == null || _client.IsDead)
            {
              _clientIntegro = new GrpcUpdaterClient<IntegroServiceClient>();  // Инициализация нового клиента
            }
          }
        }
        return _clientIntegro!;
      }
    }

    public static async Task<string?> GenerateObjectId(string prefix, long number)
    {
      //return "1111" + number.ToString("D20");
      var object_string = $"{prefix}_{number}";
      lock (_lock)
      {
        if (_idsCash.TryGetValue(object_string, out var id))
        {
          return id;
        }
      }
      GenerateObjectIdRequest generateObjectIdRequest = new GenerateObjectIdRequest();
      generateObjectIdRequest.Input.Add(new GenerateObjectIdData() { Input = object_string, Version = "1.0" });

      var result = await ClientIntegro!.Client!.GenerateObjectIdAsync(generateObjectIdRequest)  ?? null;
      var obj_id = result?.Output.FirstOrDefault()?.ObjectId;
      lock (_lock)
      {
        if (!string.IsNullOrEmpty(obj_id))
        {
          _idsCash[object_string] = obj_id;
        }             
      }
      return obj_id;
    }

    public static async Task RunTaskWithRetry(Func<Task> taskFunc, string taskName, CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        try
        {
          await taskFunc();
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
          await Task.Delay(1000, token); // Задержка перед повторной попыткой
        }
      }
    }
  }

}
