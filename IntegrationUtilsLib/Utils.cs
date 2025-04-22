using GrpcDaprLib;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.IntegroService;
using static LeafletAlarmsGrpc.TreeAlarmsGrpcService;

namespace IntegrationUtilsLib
{
  public class Utils
  {
    private static GrpcUpdaterClient<IntegroServiceClient>? _clientIntegro;
    private static GrpcUpdaterClient<TreeAlarmsGrpcServiceClient>? _clientBase;
    private static Dictionary<string,string> _idsCash = new Dictionary<string, string>();
    private static readonly object _lock = new object(); 


    public static GrpcUpdaterClient<IntegroServiceClient> ClientIntegro
    {
      get
      {
        var client = _clientIntegro;
        // Проверяем, если клиент не существует или мертв, создаем новый
        if (client != null && !client.IsDead)
        {
          return client;
        }

        lock (_lock)
        {
          _clientIntegro = new GrpcUpdaterClient<IntegroServiceClient>();
          return _clientIntegro!;
        }        
      }
    }
    public static GrpcUpdaterClient<TreeAlarmsGrpcServiceClient> ClientBase
    {
      get
      {
        var client = _clientBase;
        // Проверяем, если клиент не существует или мертв, создаем новый
        if (client != null && !client.IsDead)
        {
          return client;
        }

        lock (_lock)
        {
          _clientBase = new GrpcUpdaterClient<TreeAlarmsGrpcServiceClient>();
          return _clientBase!;
        }
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
          Logger.LogException(ex, $"taskName={taskName}");
          await Task.Delay(5000, token);
        }
      }
    }
  }

}
