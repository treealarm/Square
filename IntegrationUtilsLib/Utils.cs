using GrpcDaprLib;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.IntegroService;
using static LeafletAlarmsGrpc.TreeAlarmsGrpcService;

namespace IntegrationUtilsLib
{
  public class Utils
  {
    private static readonly Lazy<GrpcUpdaterClient<IntegroServiceClient>> _clientIntegro =
      new(() => new GrpcUpdaterClient<IntegroServiceClient>());
    private static readonly Lazy<GrpcUpdaterClient<TreeAlarmsGrpcServiceClient>> _clientBase =
      new(() => new GrpcUpdaterClient<TreeAlarmsGrpcServiceClient>());
    private static Dictionary<string,string> _idsCash = new Dictionary<string, string>();
    private static readonly object _lock = new object();

    // gRPC-канал к dapr sidecar сам переподключается при обрывах связи —
    // пересоздавать клиент-обёртку не нужно, достаточно создать один раз.
    internal static GrpcUpdaterClient<IntegroServiceClient> ClientIntegro => _clientIntegro.Value;
    internal static GrpcUpdaterClient<TreeAlarmsGrpcServiceClient> ClientBase => _clientBase.Value;

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
