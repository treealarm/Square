using Domain;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

using ObjectActions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace AASubService
{
  public class CameraManager: ICameraManager
  {
    private IntegrationSyncFull? _sync;
    private const string CamStr = "cam";
    private ConcurrentDictionary<string, Camera> _cameras = new ConcurrentDictionary<string, Camera>();
    private readonly ISubService _sub;
    private readonly ActionExecutionManager _manager = new();

    const string IpRangeParam = "ip_range";
    const string Discover = "discover";
    const string CredentialListParam = "credential_list";
    const string PortListParam = "port_list";
    public CameraManager(ISubService sub) 
    {
      _sub = sub;
    }

    public async ValueTask DisposeAsync()
    {
      if (_sync != null)
      {
        await _sync.DisposeAsync();
      }
    }
    private async Task InitCameras()
    {
      _cameras.Clear();

      var propsById = _sync!.ObjProps.Values
        .Where(prop => prop != null)
        .ToDictionary(
            prop => prop!.Id,
            prop => prop?.Properties.ToDictionary(p => p.PropName, p => p)
        );

      foreach (var prop in propsById)
      {
        if (prop.Value == null)
        { continue; }

        var ipProp = prop.Value["ip"];
        var portProp = prop.Value["port"];
        var userProp = prop.Value["user"];
        var passwordProp = prop.Value["password"];

        var cam = await Camera.CreateAsync(
          ipProp.StrVal,
          Int32.Parse(portProp.StrVal),
          userProp.StrVal,
          passwordProp.StrVal
          );
        if (cam == null)
        { continue; }
        _cameras[prop.Key] = cam;
      }
    }

    private void ConstructSync(CancellationTokenSource cancellationToken)
    {
      var type_to_props = new Dictionary<string, List<ProtoObjExtraProperty>?>();
      type_to_props.Add(IntegrationSync.MainStr, null);

      type_to_props.Add(CamStr, new List<ProtoObjExtraProperty>());

      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "ip",
        StrVal = "127.0.0.1",
        VisualType = ""
      });
      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "port",
        StrVal = "80",
        VisualType = ""
      });
      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "user",
        StrVal = "root",
        VisualType = ""
      });
      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "password",
        StrVal = "root",
        VisualType = VisualTypes.Password
      });
      _sync = new IntegrationSyncFull(_sub, type_to_props, cancellationToken.Token);
    }

    public async Task DoWork(CancellationTokenSource cancellationToken)
    {
      ConstructSync(cancellationToken);
      await _sync!.InitAll();

      var hierarchy = new Dictionary<string, IEnumerable<string>>
      {
        [IntegrationSync.MainStr] = new[] { CamStr },
        //["cam"] = new[] { "lens" },
        //["sensor"] = Array.Empty<string>(),
        //["lens"] = Array.Empty<string>()
      };

      await _sync.InitChildrenTypes(hierarchy);

      bool bInitRequest = true;

      _sync.OnConfigurationChanged += () =>
      {
        bInitRequest = true;
      };

      while (!cancellationToken.IsCancellationRequested)
      {
        if (bInitRequest)
        {
          bInitRequest = false;
          await InitCameras();
        }

        foreach (var cam in _cameras)
        {
          var media = await cam.Value.GetMediaService();
          if (media == null)
          {
            continue;
          }
          var data = await media.GetImage();
          if (data != null)
          {
            //var filePath = "M:\\snapshot.jpg";
            //await File.WriteAllBytesAsync(filePath, data);
          }
        }
        await Task.Delay(5000);
      }
    }

    public async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.CompletedTask;
      ProtoGetAvailableActionsResponse response = new ProtoGetAvailableActionsResponse();
      var action = new ProtoActionDescription
      {
        Name = Discover,
        IsLongAction = true
      };

      action.Parameters.Add(new ProtoActionParameter()
      {
        Name = IpRangeParam,
        CurVal = new ProtoActionValue()
        {
          IpRange = new ProtoIpRange()
          {
            StartIp = "127.0.0.1",
            EndIp = "127.0.0.1"
          }
        }
      });

      { 
        var credParam = new ProtoActionParameter()
        {
          Name = CredentialListParam,
          CurVal = new ProtoActionValue()
        };
        credParam.CurVal = new ProtoActionValue();
        credParam.CurVal.CredentialList = new ProtoCredentialList();
        credParam.CurVal.CredentialList.Values.Add(
          new ProtoCredential()
          { Username = "root", Password = "root" }
        );
        action.Parameters.Add(credParam);
      }

      { 
        var portsParam = new ProtoActionParameter()
        {
          Name = PortListParam,
          CurVal = new ProtoActionValue()
        };
        portsParam.CurVal = new ProtoActionValue();
        portsParam.CurVal.EnumList = new ProtoEnumList();
        portsParam.CurVal.EnumList.Values.AddRange(
          ["80", "8080"]
        );
        action.Parameters.Add(portsParam);
      }
      response.ActionsDescr.Add(action);
      return response;
    }

    public async Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request)
    {
      await Task.CompletedTask;

      foreach (var action in request.Actions)
      {
        var started = _manager.TryStartAction(action, HandleActionAsync);
        if (!started)
        {
          // Лог: задача с таким UID уже запущена
        }
      }

      return new ProtoExecuteActionResponse()
      {
        Message = "Ok",
        Success = true
      };
    }
    private async Task HandleActionAsync(ProtoActionExe action, CancellationToken token)
    {
      if (action.Name == Discover)
      {
        await HandleActionDiscoveryAsync(action, token);
      }
    }
    private async Task SendProgressAsync(ProtoActionExe action, CancellationToken token, int progress, string result)
    {
      ProtoActionExeResultRequest progressRequest = new ProtoActionExeResultRequest();
      progressRequest.Results.Add(new ProtoActionExeResult()
      {
        ActionExecutionId = action.Uid,
        Progress = progress,
        Result = result
      });
      var clientIntegro = Utils.ClientIntegro;
      await clientIntegro!.Client!.UpdateActionResultsAsync(progressRequest);
    }
    private async Task HandleActionDiscoveryAsync(ProtoActionExe action, CancellationToken token)
    {
      var ipRange = action.Parameters.Where(p => p.Name == IpRangeParam).FirstOrDefault();
      if (ipRange == null) 
      {
        return;
      }

      var startIp = IPAddress.Parse(ipRange!.CurVal.IpRange.StartIp);
      var endIp = IPAddress.Parse(ipRange!.CurVal.IpRange.EndIp);

      var range = new IpRangeEnumerator(startIp, endIp);
      var creds = action.Parameters
        .Where(p => p.Name == CredentialListParam)
        .FirstOrDefault();

      if (creds == null) 
      { 
        return; 
      }     

      int total = range.Count() * creds.CurVal.CredentialList.Values.Count;
      int step = 0;

      foreach (var ip in range)
      {
        foreach (var cred in creds.CurVal.CredentialList.Values)
        {
          int progress = (int)(step * 100.0 / total);
          step++;
          await SendProgressAsync(action, token, progress, "in progress");

          var cam = await Camera.CreateAsync(
            ip.ToString(),
            80,
            cred.Username,
            cred.Password
            );
          if (cam == null)
          { continue; }
          //_cameras[action] = cam;
        }
      }

      await SendProgressAsync(action, token, 100, "finished");
    }
  }
}
