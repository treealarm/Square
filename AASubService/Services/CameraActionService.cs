using IntegrationUtilsLib;
using ObjectActions;

namespace AASubService.Services
{
  internal class CameraActionService : IObjectActions
  {
    const string IpRangeParam = "ip_range";
    const string CredentialListParam = "credential_list";
    async public Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request)
    {
      await Task.CompletedTask;
      return new ProtoExecuteActionResponse();
    }

    async public Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.CompletedTask;
      ProtoGetAvailableActionsResponse response = new ProtoGetAvailableActionsResponse();
      var action = new ProtoActionDescription
      {
        Name = "Discover"
      };
      action.Parameters.Add(new ProtoActionParameter()
      {
        Name = nameof(IpRangeParam),
        CurVal = new ProtoActionValue()
        {
          IpRange = new ProtoIpRange()
          {
            StartIp = "127.0.0.1",
            EndIp = "127.0.0.1"
          }
        }
      });
      var credParam = new ProtoActionParameter()
      {
        Name = nameof(CredentialListParam),
        CurVal = new ProtoActionValue()
      };
      credParam.CurVal = new ProtoActionValue();
      credParam.CurVal.CredentialList = new ProtoCredentialList();
      credParam.CurVal.CredentialList.Credentials.Add(
        new ProtoCredential()
        { Username = "root", Password = "root" }
      );
      action.Parameters.Add(credParam);
      response.ActionsDescr.Add(action);
      return response;
    }
  }
}
