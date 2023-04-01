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
          ProtoFigures input = request.Data.Unpack<ProtoFigures>();

          var output = await _trackGrpcService.UpdateFigures(input, context);
          response.Data = Any.Pack(output);
          break;
        default:
          Console.WriteLine("Method not supported");
          break;
      }

      return response;
    }
  }
}

