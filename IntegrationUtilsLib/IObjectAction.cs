using ObjectActions;

namespace IntegrationUtilsLib
{
  public interface IObjectActions
  {
    Task<ProtoGetAvailableActionsResponse>
      GetAvailableActions(ProtoGetAvailableActionsRequest request);
    Task<ProtoExecuteActionResponse>
      ExecuteActions(ProtoExecuteActionRequest request);
  }
}
