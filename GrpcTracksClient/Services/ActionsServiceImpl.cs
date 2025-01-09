
using Grpc.Core;
using ObjectActions;
using static ObjectActions.ActionsService;

namespace GrpcTracksClient.Services
{
    internal class ActionsServiceImpl : ActionsServiceBase
    {
        private readonly IObjectActions _moveObjectService;
        public ActionsServiceImpl(IObjectActions moveObjectService)
        {
            _moveObjectService = moveObjectService;
        }

        public override async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(
          ProtoGetAvailableActionsRequest request,
          ServerCallContext context)
        {
            var retVal = await _moveObjectService.GetAvailableActions(request);
            return retVal;
        }

        public override async Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request, ServerCallContext context)
        {
            return await _moveObjectService.ExecuteActions(request);
        }
    }
}
