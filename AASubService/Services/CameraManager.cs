using Domain;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

using ObjectActions;
using OnvifLib;
using System.Collections.Concurrent;
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
      var ipRange = action.Parameters.FirstOrDefault(p => p.Name == IpRangeParam);
      var creds = action.Parameters.FirstOrDefault(p => p.Name == CredentialListParam);
      var portList = action.Parameters.FirstOrDefault(p => p.Name == PortListParam);

      if (ipRange == null || creds == null || portList == null)
        return;

      var ipStart = ipRange.CurVal.IpRange.StartIp;
      var ipEnd = ipRange.CurVal.IpRange.EndIp;
      var ports = portList.CurVal.EnumList.Values.Select(int.Parse).ToList();
      var credentials = creds.CurVal.CredentialList.Values
          .Select(c => (c.Username, c.Password)).ToList();

      await CameraScanner.ScanAsync(
          ipStart,
          ipEnd,
          ports,
          credentials,
          onProgress: (progress, status) => SendProgressAsync(action, token, progress, status),
          onCameraDiscovered: cam => CreateCameraIntegration(cam),
          token: token,
          _cameras.Values.ToList()
          );
    }


    private async Task CreateCameraIntegration(Camera cam)
    {
      var existing = _cameras.Where(c => c.Value.Url == cam.Url).FirstOrDefault();

      string? cam_id = string.Empty;

      if (existing.Value != null)
      {
      }
      else
      {
        cam_id = await Utils.GenerateObjectId($"{cam.Ip}:{cam.Port}", 1);
      }
      var clientBase = Utils.ClientBase.Client;
      if (clientBase == null) 
      {
        return;
      }
      var props = new ProtoObjPropsList();

      var ipProp = new ProtoObjProps()
      {
        Id = existing.Key
      };
      props.Objects.Add(ipProp);

      ipProp.Properties.Add(new ProtoObjExtraProperty()
      {
        PropName = "ip",
        StrVal = cam.Ip
      });
      ipProp.Properties.Add(new ProtoObjExtraProperty()
      {
        PropName = "port",
        StrVal = cam.Port.ToString()
      });
      ipProp.Properties.Add(new ProtoObjExtraProperty()
      {
        PropName = "user",
        StrVal = cam.User
      });
      ipProp.Properties.Add(new ProtoObjExtraProperty()
      {
        PropName = "password",
        StrVal = cam.Password
      });

      await clientBase.UpdatePropertiesAsync(props);
      
      //_sync.InitMainObject
    }
  }
}
