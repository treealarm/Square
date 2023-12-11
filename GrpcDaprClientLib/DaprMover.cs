using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDaprClientLib;
using LeafletAlarmsGrpc;


namespace GrpcDaprLib
{
  public class DaprMover : IDisposable
  {
    public static string GetDaprProxyUrl()
    {
      var allVars = Environment.GetEnvironmentVariables();

      foreach (var key in allVars.Keys)
      {
        Console.WriteLine($"{key}- {allVars[key]}");
      }
      if (int.TryParse(Environment.GetEnvironmentVariable("DAPR_CLI_PORT"), out var DAPR_CLI_PORT))
      {
        Console.WriteLine($"DAPR_CLI_PORT port:{DAPR_CLI_PORT}");
        var builder = new UriBuilder("http", "grpctracksclient-dapr", DAPR_CLI_PORT);
        Console.WriteLine(builder.ToString());
        return builder.ToString();
      }
      Console.Error.WriteLine("GetDaprProxyPort return empty string");
      return string.Empty;
    }

    private GrpcMover? _daprClient = null;
    public DaprMover()
    {      
      _daprClient = new GrpcMover();      
    }
    public void Dispose()
    {
      if (_daprClient != null)
      {
        _daprClient.Dispose();
      }
    }

    public async Task<ProtoFigures?> Move(ProtoFigures figs)
    {
      if (_daprClient == null)
      {
        return null;
      }

      if (!_daprClient.IsConnected())
      {
        _daprClient.Connect(GetDaprProxyUrl());
      }
      

      var metadata = new Metadata
      {
        { "dapr-app-id", "leafletalarms" }
      };

      var reply = await _daprClient.Move(figs, metadata);

      return reply;
    }
  }
}
