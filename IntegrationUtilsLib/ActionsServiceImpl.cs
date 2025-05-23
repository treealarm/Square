
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ObjectActions;
using static ObjectActions.ActionsService;

namespace IntegrationUtilsLib
{
  public class ActionsServiceImpl : ActionsServiceBase
  {
    private readonly IObjectActions _actionServiceInternal;
    public ActionsServiceImpl(IObjectActions actionService)
    {
      _actionServiceInternal = actionService;
    }

    public override async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(
      ProtoGetAvailableActionsRequest request,
      ServerCallContext context)
    {
      var retVal = await _actionServiceInternal.GetAvailableActions(request);
      return retVal;
    }

    public override async Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request, ServerCallContext context)
    {
      return await _actionServiceInternal.ExecuteActions(request);
    }
    public override async Task<BoolValue> CancelActions(ProtoEnumList request, ServerCallContext context)
    {
      return await _actionServiceInternal.CancelActions(request);
    }
  }
}
