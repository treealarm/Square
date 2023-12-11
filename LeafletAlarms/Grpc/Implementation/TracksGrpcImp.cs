using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.States;
using Domain.StateWebSock;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarms.Services;
using LeafletAlarmsGrpc;
using StackExchange.Redis;
using System.Text.Json;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace LeafletAlarms.Grpc.Implementation
{
  public class TracksGrpcImp: TracksGrpcServiceBase
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
  }
}
