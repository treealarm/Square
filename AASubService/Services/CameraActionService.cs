using IntegrationUtilsLib;
using ObjectActions;

namespace AASubService
{
  internal class CameraActionService : IObjectActions
  {
    private readonly ICameraManager _cameraManager;
    public CameraActionService(ICameraManager cameraManager) 
    {
      _cameraManager = cameraManager;
    }
    
    async public Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request)
    {
      return await _cameraManager.ExecuteActions(request);      
    }

    async public Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      return await _cameraManager.GetAvailableActions(request);
    }
  }
}
