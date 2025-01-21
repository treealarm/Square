using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.IntegroService;
using static LeafletAlarmsGrpc.TreeAlarmsGrpcService;

namespace GrpcDaprLib
{
  public class GrpcUpdater : IDisposable
  {    
    public string AppId { get; set; } = string.Empty;
    private GrpcChannel? _channel = null;
    private TreeAlarmsGrpcServiceClient? _client = null;
    private IntegroServiceClient? _integroClient = null;

    private CallInvoker? _daprClient = null;
    public bool IsDead
    {
      get
      {
        return _integroClient != null && _integroClient != null;
      } 
    }

    public static int GetAppPort(string env_name = "APP_PORT",int def_val = 5001)
    {
      //var allVars = Environment.GetEnvironmentVariables();
      if (int.TryParse(Environment.GetEnvironmentVariable(env_name), out var port))
      {
        Console.WriteLine($"{env_name} port:{port}");
        return port;
      }
      Console.Error.WriteLine("{env_name} return empty string");
      return def_val;
    }

    public GrpcUpdater()
    {
      _daprClient = DaprClient.CreateInvocationInvoker(appId: "leafletalarms");
      _client = new TreeAlarmsGrpcServiceClient(_daprClient);
      _integroClient = new IntegroServiceClient (_daprClient);

      AppId = Environment.GetEnvironmentVariable("APP_ID") ?? string.Empty;
      Console.WriteLine($"APP_ID:{AppId}");    
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

    public async Task<bool?> UpdateIntegro(UpdateIntegroRequest integro)
    {
      if (_integroClient == null) return null;

      var result = await _integroClient.UpdateIntegroAsync(integro);
      return result.Value;
    }

    public async Task<string?> GenerateObjectId(string input)
    {
      if (_integroClient == null) return null;

      GenerateObjectIdRequest generateObjectIdRequest = new GenerateObjectIdRequest();
      generateObjectIdRequest.Input.Add(new GenerateObjectIdData() { Input = input , Version = "1.0"});
      var result = await _integroClient.GenerateObjectIdAsync(generateObjectIdRequest);
      return result.Output.FirstOrDefault()?.ObjectId;
    }
  }
}
