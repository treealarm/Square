using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.TreeAlarmsGrpcService;

namespace GrpcDaprLib
{
  public class GrpcUpdater : IDisposable
  {    
    private GrpcChannel? _channel = null;
    private TreeAlarmsGrpcServiceClient? _client = null;

    private CallInvoker? _daprClient = null;

    public static int GetGrpcAppPort()
    {
      var allVars = Environment.GetEnvironmentVariables();
      if (int.TryParse(Environment.GetEnvironmentVariable("APP_PORT"), out var GRPC_CLIENT_PORT))
      {
        Console.WriteLine($"GRPC_CLIENT_PORT port:{GRPC_CLIENT_PORT}");
        var builder = new UriBuilder("http", "leafletalarmsservice", GRPC_CLIENT_PORT);

        return GRPC_CLIENT_PORT;
      }
      Console.Error.WriteLine("GRPC_CLIENT_PORT return empty string");
      return 5001;
    }

    public GrpcUpdater(string? endPoint = null)
    {
      _daprClient = DaprClient.CreateInvocationInvoker(appId: "leafletalarms");
      _client = new TreeAlarmsGrpcServiceClient(_daprClient);
    }

    public void Dispose()
    {
      _channel?.Dispose();
      _channel = null;
      _client = null;
    }

    public async Task<ProtoFigures?> Move(ProtoFigures figs, Metadata? meta = null)
    {
      if (_client == null) return null;
      try
      {
        var newFigs = await _client.UpdateFiguresAsync(figs, meta);
        return newFigs;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      return null;
    }

    public async Task<bool?> UpdateTracks(TrackPointsProto figs)
    {
      if (_client == null) return null;

      var newFigs = await _client.UpdateTracksAsync(figs);
      return newFigs.Value;
    }

    public async Task SendStates(ProtoObjectStates states)
    {
      if (_client != null)
      {
        await _client.UpdateStatesAsync(states);
      }
    }

    public async Task<bool?> AddEvents(EventsProto events)
    {
      if (_client == null) return null;

      var result = await _client.UpdateEventsAsync(events);
      return result.Value;
    }

    public async Task<ValuesProto?> UpdateValues(ValuesProto events)
    {
      if (_client == null) return null;

      var result = await _client.UpdateValuesAsync(events);
      return result;
    }

    public async Task<bool?> UploadFile(UploadFileProto file_data)
    {
      if (_client == null) return null;

      var result = await _client.UploadFileAsync(file_data);
      return result.Value;
    }

    public async Task<DiagramTypesProto?> UpdateDiagramTypes(DiagramTypesProto d_type)
    {
      if (_client == null) return null;

      var result = await _client.UpdateDiagramTypesAsync(d_type);
      return result;
    }

    public async Task<DiagramsProto?> UpdateDiagrams(DiagramsProto diag)
    {
      if (_client == null) return null;

      var result = await _client.UpdateDiagramsAsync(diag);
      return result;
    }
  }
}
