using Grpc.Core;
using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using System.Threading.Channels;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace GrpcDaprClientLib
{
  public class GrpcMover : IDisposable
  {
    private GrpcChannel? _channel = null;
    private TracksGrpcServiceClient? _client = null;

    public static string GetGrpcMainUrl()
    {
      var allVars = Environment.GetEnvironmentVariables();

      foreach (var key in allVars.Keys)
      {
        Console.WriteLine($"{key}- {allVars[key]}");
      }
      if (int.TryParse(Environment.GetEnvironmentVariable("GRPC_MAIN_PORT"), out var GRPC_MAIN_PORT))
      {
        Console.WriteLine($"GRPC_MAIN_PORT port:{GRPC_MAIN_PORT}");
        var builder = new UriBuilder("http", "leafletalarmsservice", GRPC_MAIN_PORT);
        Console.WriteLine(builder.ToString());
        return builder.ToString();
      }
      Console.Error.WriteLine("GetDaprProxyPort return empty string");
      return string.Empty;
    }

    public string GRPC_DST { get; private set; } = string.Empty;// = $"http://leafletalarmsservice:5000";
    public void Connect(string endPoint)
    {
      GRPC_DST = GetGrpcMainUrl();

      if (!string.IsNullOrEmpty(endPoint))
      {
        GRPC_DST = endPoint;
      }

      _channel = GrpcChannel.ForAddress(GRPC_DST);
      _client = new TracksGrpcServiceClient(_channel);
    }

    public bool IsConnected()
    {
      return _client != null;
    }
    public void Dispose()
    {
      if (_channel != null)
      {
        _channel.Dispose();
        _channel = null;
      }
      _client = null;
    }

    public async Task<ProtoFigures?> Move(ProtoFigures figs, Metadata? meta = null)
    {
      if (_client == null)
      {
        return null;
      }
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
      if (_client == null)
      {
        return null;
      }

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
  }
}