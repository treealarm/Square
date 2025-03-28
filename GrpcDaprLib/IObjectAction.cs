using ObjectActions;

namespace GrpcDaprLib
{
  public interface IObjectActions
  {
    Task<ProtoGetAvailableActionsResponse>
      GetAvailableActions(ProtoGetAvailableActionsRequest request);
    Task<ProtoExecuteActionResponse>
      ExecuteActions(ProtoExecuteActionRequest request);
  }
}
