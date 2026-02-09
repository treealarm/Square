using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace GrpcDaprLib
{
  public abstract class GrpcBaseUpdater : IDisposable
  {
    protected GrpcChannel? _channel;
    protected CallInvoker? _daprClient;
    public string AppId { get; private set; } = string.Empty;

    public GrpcBaseUpdater()
    {
      AppId = Environment.GetEnvironmentVariable("APP_ID") ?? string.Empty;

      if (string.IsNullOrEmpty(AppId))
      {
        Console.Error.WriteLine("AppId is empty dapr will not work");
      }
      Console.WriteLine($"APP_ID: {AppId}");

      var LEAFLETALARM_APP_ID = Environment.GetEnvironmentVariable("LEAFLETALARM_APP_ID") ?? "leafletalarms";
      _daprClient = DaprClient.CreateInvocationInvoker(LEAFLETALARM_APP_ID);
      Console.WriteLine($"LEAFLETALARM_APP_ID: {LEAFLETALARM_APP_ID}");
    }

    public static int GetAppPort(string env_name, int def_val)
    {
      if (int.TryParse(Environment.GetEnvironmentVariable(env_name), out var port))
      {
        Console.WriteLine($"{env_name} port: {port}");
        return port;
      }
      Console.Error.WriteLine($"{env_name} return empty string using default {def_val}");
      return def_val;
    }

    public virtual void Dispose()
    {
      _channel?.Dispose();
      _channel = null;
    }
  }
}
