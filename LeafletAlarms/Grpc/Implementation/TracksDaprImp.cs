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
    TracksGrpcImp _trackGrpcService;
    public TracksDaprImp(ILogger<TracksDaprImp> logger, TracksGrpcImp trackGrpcService)
    {
      _logger = logger;
      _trackGrpcService = trackGrpcService;
    }

    public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
    {
      InvokeResponse response = new();

      switch (request.Method)
      {
        case "UpdateFigures":
          var input = request.Data.Unpack<ProtoFigures>();
          var output = await _trackGrpcService.UpdateFigures(input, context);
          response.Data = Any.Pack(output);
          break;

        case "UpdateStates":
          var states = request.Data.Unpack<ProtoObjectStates>();
          var stateRet = await _trackGrpcService.UpdateStates(states, context);
          response.Data = Any.Pack(stateRet);
          break;

        case "UpdateTracks":
          var tracks = request.Data.Unpack<TrackPointsProto>();
          var tracksRet = await _trackGrpcService.UpdateTracks(tracks, context);
          response.Data = Any.Pack(tracksRet);
          break;

        default:
          Console.WriteLine("Method not supported");
          break;
      }

      return response;
    }
  }
}

