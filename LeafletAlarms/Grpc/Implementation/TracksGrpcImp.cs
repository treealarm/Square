using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.TreeAlarmsGrpcService;

namespace LeafletAlarms.Grpc.Implementation
{
  public class TracksGrpcImp: TreeAlarmsGrpcServiceBase
  {
    private readonly GRPCServiceProxy _proxy;
    public TracksGrpcImp(
      GRPCServiceProxy proxy
    )
    {
      _proxy = proxy;
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

    public override async Task<ValuesProto> UpdateValues(ValuesProto request, ServerCallContext context)
    {
      return await _proxy.UpdateValues(request);
    }
    public override async Task<DiagramTypesProto> UpdateDiagramTypes(DiagramTypesProto request, ServerCallContext context)
    {
      return await _proxy.UpdateDiagramTypes(request);
    }
    public override async Task<DiagramsProto> UpdateDiagrams(DiagramsProto request, ServerCallContext context)
    {
      return await _proxy.UpdateDiagrams(request);
    }

    public override async Task<BoolValue> UploadFile(UploadFileProto request, ServerCallContext context)
    {
      return await _proxy.UploadFile(request);
    }

    public override async Task<ProtoObjectList> UpdateObjects(ProtoObjectList request, ServerCallContext context)
    {
      return await _proxy.UpdateObjects(request);
    }
    public override async Task<ProtoObjectList> RequestObjects(ProtoObjectIds request, ServerCallContext context)
    {
      return await _proxy.RequestObjects(request);
    }
  }
}
