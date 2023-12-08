using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using System.Threading.Channels;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace GrpcDaprClientLib
{
  public class GrpcMover : IMove, IDisposable
  {
    private GrpcChannel? _channel = null;
    private TracksGrpcServiceClient? _client = null;
    public string GRPC_DST { get; private set; } = $"http://leafletalarmsservice:5000";
    public void Connect(string endPoint)
    {
      if (!string.IsNullOrEmpty(endPoint))
      {
        GRPC_DST = endPoint;
      }
      _channel = GrpcChannel.ForAddress(GRPC_DST);
      _client = new TracksGrpcServiceClient(_channel);
    }

    public void Dispose()
    {
      if (_channel != null)
      {
        _channel.Dispose();
        _channel = null;
      }
    }

    public async Task<ProtoFigures?> Move(ProtoFigures figs)
    {
      if (_client == null)
      {
        return null;
      }
      try
      {
        var newFigs = await _client.UpdateFiguresAsync(figs);
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