using IntegrationUtilsLib;
using ObjectActions;

namespace AASubService.Services
{
  internal class CameraService : IObjectActions
  {
    async public Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request)
    {
      await Task.CompletedTask;
      return new ProtoExecuteActionResponse();
    }

    async public Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.CompletedTask;
      return new ProtoGetAvailableActionsResponse();
    }
  }
}
