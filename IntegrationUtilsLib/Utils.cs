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

    // На старте sidecar dapr может быть ещё не готов — конструктор GrpcUpdaterClient
    // бросит исключение. В этом случае поле остаётся null, и следующий доступ
    // повторит попытку создания (Lazy<T> в этом месте кэширует исключение навсегда —
    // именно это и ломало переподключение). После успешного создания канал к dapr
    // sidecar сам переподключается при обрывах связи — пересоздавать не нужно.
    internal static GrpcUpdaterClient<IntegroServiceClient> ClientIntegro
    {
      get
      {
        if (_clientIntegro != null) { return _clientIntegro; }
        lock (_lock)
        {
          _clientIntegro ??= new GrpcUpdaterClient<IntegroServiceClient>();
          return _clientIntegro;
        }
      }
    }
    internal static GrpcUpdaterClient<TreeAlarmsGrpcServiceClient> ClientBase
    {
      get
      {
        if (_clientBase != null) { return _clientBase; }
        lock (_lock)
        {
          _clientBase ??= new GrpcUpdaterClient<TreeAlarmsGrpcServiceClient>();
          return _clientBase;
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
