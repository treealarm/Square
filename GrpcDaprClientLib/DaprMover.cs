using Dapr.Client;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDaprClientLib;
using LeafletAlarmsGrpc;


namespace GrpcDaprLib
{
  public class DaprMover : IDisposable
  {
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
        _daprClient.Connect("http://grpctracksclient-dapr:50007");
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
