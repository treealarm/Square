using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarmsGrpc;

namespace LeafletAlarms.Grpc.Implementation
{
  public class TracksDaprImp : AppCallback.AppCallbackBase
  {
    private readonly ILogger<TracksDaprImp> _logger;
    GRPCServiceProxy _proxy;

    public TracksDaprImp(ILogger<TracksDaprImp> logger, GRPCServiceProxy proxy)
    {
      _logger = logger;
      _proxy = proxy;
    }

    public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
    {
      InvokeResponse response = new();

      try
      {
        Console.WriteLine($"Method {request.Method} is about to be called");

        switch (request.Method)
        {
          case "UpdateFigures":
            var input = request.Data.Unpack<ProtoFigures>();
            var output = await _proxy.UpdateFigures(input);
            response.Data = Any.Pack(output);
            break;

          case "UpdateStates":
            var states = request.Data.Unpack<ProtoObjectStates>();
            var stateRet = await _proxy.UpdateStates(states);
            response.Data = Any.Pack(stateRet);
            break;

          case "UpdateTracks":
            var tracks = request.Data.Unpack<TrackPointsProto>();
            var tracksRet = await _proxy.UpdateTracks(tracks);
            response.Data = Any.Pack(tracksRet);
            break;

          default:
            Console.WriteLine("Method not supported");
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }


      return response;
    }
  }
}

