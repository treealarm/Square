
using Grpc.Core;
using ObjectActions;
using static ObjectActions.ActionsService;

namespace GrpcTracksClient
{
  internal class ActionsServiceImpl : ActionsServiceBase
  {
    public override async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(
      ProtoGetAvailableActionsRequest request, 
      ServerCallContext context)
    {
      var retVal = new ProtoGetAvailableActionsResponse();
      retVal.Actions.Add(new ProtoAction() { Name = "test1"});
      retVal.Actions.Add(new ProtoAction() { Name = "test2" });
      return retVal;
    }

    public override async Task<ProtoExecuteActionResponse> ExecuteAction(ProtoExecuteActionRequest request, ServerCallContext context)
    {
      var retVal = new ProtoExecuteActionResponse();
      return retVal;
    }
  }
}
