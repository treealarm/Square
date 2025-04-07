using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcDaprLib
{
  public abstract class GrpcBaseUpdater : IDisposable
  {
    protected GrpcChannel? _channel;
    protected CallInvoker? _daprClient;
    public string AppId { get; private set; } = string.Empty;

    public GrpcBaseUpdater(string appId = "leafletalarms")
    {
      _daprClient = DaprClient.CreateInvocationInvoker(appId);
      AppId = Environment.GetEnvironmentVariable("APP_ID") ?? string.Empty;
      Console.WriteLine($"APP_ID: {AppId}");
    }

    public static int GetAppPort(string env_name = "APP_PORT", int def_val = 5001)
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
