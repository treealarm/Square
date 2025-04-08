using IntegrationUtilsLib;
using ObjectActions;

namespace AASubService.Services
{
  internal class CameraService : IObjectActions
  {
    async public Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request)
    {
      return new ProtoExecuteActionResponse();
    }

    async public Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      return new ProtoGetAvailableActionsResponse();
    }
  }
}
