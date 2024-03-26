using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarmsGrpc;
using Microsoft.Extensions.Logging;
using static LeafletAlarmsGrpc.TreeAlarmsGrpcService;

namespace LeafletAlarms.Grpc.Implementation
{
  public class TracksGrpcImp: TreeAlarmsGrpcServiceBase
  {
    private readonly ILogger<TracksGrpcImp> _logger;
    private readonly GRPCServiceProxy _proxy;
    public TracksGrpcImp(
      ILogger<TracksGrpcImp> logger,
      GRPCServiceProxy proxy
    )
    {
      _proxy = proxy;
      _logger = logger;
    }

    public override async Task<ProtoFigures> UpdateFigures(ProtoFigures request, ServerCallContext context)
    {   
      return await _proxy.UpdateFigures(request);
    }

    public override async Task<BoolValue> UpdateStates(ProtoObjectStates request, ServerCallContext context)
    {
      return await _proxy.UpdateStates(request);
    }

    public override async Task<BoolValue> UpdateTracks(TrackPointsProto request, ServerCallContext context)
    {
      return await _proxy.UpdateTracks(request);
    }

    public override async Task<BoolValue> UpdateEvents(EventsProto request, ServerCallContext context)
    {
      return await _proxy.UpdateEvents(request);
    }
  }
}
