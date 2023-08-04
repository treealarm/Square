using Dapr.Client;
using GrpcDaprClientLib;
using LeafletAlarmsGrpc;


namespace GrpcDaprLib
{
  public class DaprMover : IMove, IDisposable
  {
    private DaprClient? _daprClient = null;
    public DaprMover()
    {
      _daprClient = new DaprClientBuilder().Build();
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

      var reply =
            await _daprClient.InvokeMethodGrpcAsync<ProtoFigures, ProtoFigures>(
              "leafletalarms",
              "AddTracks",
              figs
            );

      return reply;
    }
  }
}
