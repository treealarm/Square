using AASubService.Services;
using Common;
using Domain;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

using ObjectActions;
using OnvifLib;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AASubService
{
  public class CameraManager : ICameraManager
  {
    private IntegrationSyncFull? _sync;
    private const string CamStr = "cam";
    private ConcurrentDictionary<string, Camera> _cameras = new ConcurrentDictionary<string, Camera>();
    private readonly ISubService _sub;
    private readonly ActionExecutionManager _manager = new();
    private readonly CameraEventServiceManager _cameraEventServiceManager;

    const string IpRangeParam = "ip_range";
    const string Discover = "discover";
    const string Telemetry = "telemetry";
    const string Refresh = "refresh";
    const string GetSnapshot = "get_snapshot";
    const string CredentialListParam = "credential_list";
    const string PortListParam = "port_list";
    public CameraManager(ISubService sub, CameraEventServiceManager cameraEventServiceManager)
    {
      _sub = sub;
      _cameraEventServiceManager = cameraEventServiceManager;
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
      var propsById = _sync!.ObjProps.Values
          .Where(prop => prop != null)
          .ToDictionary(
              prop => prop!.Id,
              prop => prop?.Properties.ToDictionary(p => p.PropName, p => p)
          );

      // Удаляем камеры, которых больше нет в props
      var removedIds = _cameras.Keys.Except(propsById.Keys).ToList();
      foreach (var id in removedIds)
      {
        if (_cameras.TryRemove(id, out var toRemove))
        {
          await _cameraEventServiceManager.RemoveCameraAsync(id);
        }
      }

      // Обновляем или добавляем камеры
      foreach (var kv in propsById)
      {
        var id = kv.Key;
        var dict = kv.Value!;

        dict.TryGetValue("ip", out var ipProp);
        dict.TryGetValue("port", out var portProp);
        dict.TryGetValue("user", out var userProp);
        dict.TryGetValue("password", out var passwordProp);

        var ip = ipProp?.StrVal ?? "127.0.0.1";
        var portStr = portProp?.StrVal ?? "80";
        var user = userProp?.StrVal ?? "root";
        var password = passwordProp?.StrVal ?? "root";

        if (!int.TryParse(portStr, out var port))
          port = 80;

        // Проверяем, есть ли уже такая камера с теми же параметрами
        if (_cameras.TryGetValue(id, out var existingCam))
        {
          if (existingCam.Ip == ip &&
              existingCam.Port == port &&
              existingCam.User == user &&
              existingCam.Password == password)
          {
            // Ничего не изменилось → пропускаем
            continue;
          }

          // Если что-то изменилось → удаляем старую
          if (_cameras.TryRemove(id, out var toRemove))
          {
            await _cameraEventServiceManager.RemoveCameraAsync(id);
          }
        }

        // Создаём новую камеру
        var cam = Camera.Create(ip, port, user, password);
        _cameras[id] = cam;
        await _cameraEventServiceManager.AddCameraAsync(id, cam);
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
      var configChangedTcs = new TaskCompletionSource();

      _sync.OnConfigurationChanged += () =>
      {
        bInitRequest = true;
        // сбрасываем TCS, если оно ещё не выполнено
        if (!configChangedTcs.Task.IsCompleted)
          configChangedTcs.TrySetResult();
      };

      while (!cancellationToken.IsCancellationRequested)
      {
        if (bInitRequest)
        {
          bInitRequest = false;
          await InitCameras();
        }

        var events = new EventsProto();

        foreach (var cam in _cameras)
        {
          try
          {
            var res = await UploadCamImage(cam);
            if (!res)
              events.Events.Add(await CreateNoImageEvent(cam));
          }
          catch (Exception ex)
          {
            events.Events.Add(await CreateNoImageEvent(cam));
            Console.WriteLine(ex);
          }
        }

        try
        {
          var client = Utils.ClientBase.Client;
          if (client != null && events.Events.Any())
            await client.UpdateEventsAsync(events);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }

        // ждём либо 30 секунд, либо событие изменения
        var delayTask = Task.Delay(30000, cancellationToken.Token);
        var changeTask = configChangedTcs.Task;

        var finished = await Task.WhenAny(delayTask, changeTask);

        if (finished == changeTask)
        {
          // конфиг изменился, ждём ещё 2 секунды (debounce)
          try
          {
            await Task.Delay(2000, cancellationToken.Token);
          }
          catch (TaskCanceledException) { }

          // сбрасываем TCS на новый
          configChangedTcs = new TaskCompletionSource();
        }
      }
    }
    async private Task<EventProto> CreateNoImageEvent(KeyValuePair<string, Camera> cam)
    {
      var newEv = new EventProto();
      newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);


      newEv.EventPriority = (int)LogLevel.Error;
      newEv.EventName = "no image";

      //newEv.ExtraProps.Add(new ProtoObjExtraProperty()
      //{
      //  PropName = "license_image",
      //  StrVal = Convert.ToBase64String(imageArray),
      //  VisualType = "base64image_fs"
      //});

      newEv.ObjectId = cam.Key;

      return newEv;
    }
    async private Task<bool> UploadCamImage(KeyValuePair<string, Camera> cam)
    {
      var media = await cam.Value.GetMediaService();
      if (media == null)
      {
        return false;
      }
      var data = await media.GetImage();
      if (data != null)
      {
        //var filePath = "M:\\snapshot.jpg";
        //await File.WriteAllBytesAsync(filePath, data);
        var protoUploadFile = new UploadFileProto()
        {
          MainFolder = "static_files",
          Path = "object_images",
          FileName = $"{cam.Key}{data.Extension}",
        };

        protoUploadFile.FileData = Google.Protobuf.ByteString.CopyFrom(data.Data);

        // Создаем клиента gRPC и подключаемся
        var client = Utils.ClientBase.Client;

        if (client == null)
        {
          await Task.Delay(1000);
          return false;
        }
        // Загружаем файл
        await client.UploadFileAsync(protoUploadFile);

        var toSend = new ProtoObjPropsList();


        var protoProp = new ProtoObjProps()
        {
          Id = cam.Key
        };
        protoProp.Properties.Add(new ProtoObjExtraProperty()
        {
          PropName = "__snapshot",
          StrVal = Path.Combine([protoUploadFile.MainFolder, protoUploadFile.Path, protoUploadFile.FileName]),
          VisualType = VisualTypes.SnapShot
        });
        toSend.Objects.Add(protoProp);

        await client!.UpdatePropertiesAsync(toSend);
        return true;
      }
      return false;
    }

    public async Task<ProtoGetAvailableActionsResponse> GetMainAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.CompletedTask;
      ProtoGetAvailableActionsResponse response = new ProtoGetAvailableActionsResponse();

      var action = new ProtoActionDescription
      {
        Name = Discover
      };

      action.Parameters.Add(new ProtoActionParameter()
      {
        Name = IpRangeParam,
        CurVal = new ProtoActionValue()
        {
          IpRange = new ProtoIpRange()
          {
            StartIp = "172.16.254.136",
            EndIp = "172.16.254.136"
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
        credParam.CurVal.CredentialList.Values.AddRange(
          [
          new ProtoCredential() { Username = "orwell", Password = "elvees" }
          ]
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
          ["80"]
        );
        action.Parameters.Add(portsParam);
      }
      response.ActionsDescr.Add(action);
      return response;
    }

    public async Task<ProtoGetAvailableActionsResponse> GetCamAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.CompletedTask;
      ProtoGetAvailableActionsResponse response = new ProtoGetAvailableActionsResponse();

      {
        var action = new ProtoActionDescription
        {
          Name = Telemetry
        };

        var param = new ProtoActionParameter()
        {
          Name = "move",
          CurVal = new ProtoActionValue()
          {
            Map = new ProtoMap()
            {
            }
          }
        };
        param.CurVal.Map.Values.Add(new Dictionary<string, string>()
        {
          { "pan", "0" },
          { "tilt", "0" },
          { "speed", "1" },
          { "zoom", "0" },
          { "move_type", "relative" }
        });
        action.Parameters.Add(param);
        response.ActionsDescr.Add(action);
      }
      {
        var action = new ProtoActionDescription
        {
          Name = Refresh
        };
        response.ActionsDescr.Add(action);
      }
      {
        var action = new ProtoActionDescription
        {
          Name = GetSnapshot
        };
        response.ActionsDescr.Add(action);
      }
      
      return response;
    }
    public async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      if (_sync == null || _sync.MainObj == null)
      {
        return new ProtoGetAvailableActionsResponse();
      }
      if (request.ObjectId == _sync!.MainObj!.Id)
      {
        return await GetMainAvailableActions(request);
      }

      if (_cameras.TryGetValue(request.ObjectId, out var cameras))
      {
        return await GetCamAvailableActions(request);
      }

      return new ProtoGetAvailableActionsResponse();
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
      if (action.Name == Refresh)
      {
        await HandleActionRefreshAsync(action, token);
      }
      if (action.Name == Telemetry)
      {
        await HandleActionTelemetryAsync(action, token);
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
    private async Task HandleActionRefreshAsync(ProtoActionExe action, CancellationToken token)
    {
      if (_cameras.TryGetValue(action.ObjectId, out var camera))
      {
        var pair = new KeyValuePair<string, Camera>(action.ObjectId, camera);
        try
        {
          await UploadCamImage(pair);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }
    private async Task<ProtoExecuteActionResponse> HandleActionGetSnapshotAsync(ProtoActionExe action)
    {
      var ret = new ProtoExecuteActionResponse();
      if (_cameras.TryGetValue(action.ObjectId, out var cam))
      {
        try
        {
          var media = await cam.GetMediaService();
          if (media == null)
          {
            return ret;
          }
          var data = await media.GetImage();
          if (data != null)
          {
            ret.Success = true;
            ret.Message = "Ok";
            ret.MimeType = data.MimeType;
            ret.Data = ByteString.CopyFrom(data.Data);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }        
      }
      return ret;
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

      try
      {
        await CameraScanner.ScanAsync(
          ipStart,
          ipEnd,
          ports,
          credentials,
          onProgress: (progress, status) => SendProgressAsync(action, token, progress, status),
          onCameraDiscovered: (cam, action) => CreateCameraIntegration(cam, action),
          token: token,
          _cameras.Values.ToList(),
          action
          );
      }
      catch (Exception ex)
      {
        await SendProgressAsync(action, token, 100, ex.ToString());
      }
    }
    private async Task HandleActionTelemetryAsync(ProtoActionExe action, CancellationToken token)
    {
      if (!_cameras.TryGetValue(action.ObjectId, out var camera))
        return;

      var ptz = await camera.GetPtzService();
      if (ptz == null)
        return;

      var directions = action.Parameters
        .FirstOrDefault(p => p.Name == "move")?
        .CurVal?.Map?.Values;

      if (directions == null || directions.Count == 0)
        return;

      float pan = 0f;
      float tilt = 0f;
      float zoom = 0f;
      string move_type = "relative";

      float ParseOrDefault(string s, float defaultValue = 0f)
      {
        return float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
      }

      foreach (var dict in directions)
      {
        string key = dict.Key.ToLowerInvariant();
        string value = dict.Value;

        switch (key)
        {
          case "tilt":
            tilt = ParseOrDefault(value);
            break;
          case "pan":
            pan = ParseOrDefault(value);
            break;
          case "zoom":
            zoom = ParseOrDefault(value);
            break;
          case "move_type":
            move_type = value;
            break;
        }
      }


      try
      {
        var profiles = await camera.GetProfiles();
        if (move_type == "relative")
        {
          await ptz.RelativeMoveAsync(profiles?.FirstOrDefault() ?? "", pan, tilt, zoom);
        }          
        else if(move_type == "absolute")
        {
          await ptz.AbsoluteMoveAsync(profiles?.FirstOrDefault() ?? "", pan, tilt, zoom);
        }
        else if (move_type == "continuous")
        {
          await ptz.ContinuousMoveAsync(profiles?.FirstOrDefault() ?? "", pan, tilt, zoom);
        }
        else
        {
          await ptz.StopAsync(profiles?.FirstOrDefault() ?? "");
        }
        //await Task.Delay(100, token);
        //await ptz.StopAsync(profiles?.FirstOrDefault() ?? "", true, true);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Telemetry move failed: {ex}");
      }
    }



    private async Task CreateCameraIntegration(Camera cam, object context)
    {
      var cam_prop = await BuildCameraPropsAsync(cam);
      var name = $"cam {cam.Ip}:{cam.Port}";
      await _sync!.CreateFullObject(cam_prop, name, CamStr);
    }
    private async Task<ProtoObjProps> BuildCameraPropsAsync(Camera cam)
    {
      var existing = _cameras.FirstOrDefault(c => c.Value.Url == cam.Url);

      string? objId = existing.Value != null
          ? existing.Key
          : await Utils.GenerateObjectId($"{cam.Ip}:{cam.Port}", 1);

      var ipProp = new ProtoObjProps
      {
        Id = objId
      };

      ipProp.Properties.Add(new ProtoObjExtraProperty { PropName = "ip", StrVal = cam.Ip });
      ipProp.Properties.Add(new ProtoObjExtraProperty { PropName = "port", StrVal = cam.Port.ToString() });
      ipProp.Properties.Add(new ProtoObjExtraProperty { PropName = "user", StrVal = cam.User });
      ipProp.Properties.Add(new ProtoObjExtraProperty { PropName = "password", StrVal = cam.Password });
      return ipProp;
    }

    public async Task<BoolValue> CancelActions(ProtoEnumList request)
    {
      var retVal = new BoolValue() { Value = true };
      foreach (var action_id in request.Values)
      {
        if (!await _manager.CancelAction(action_id))
        {
          retVal.Value = false;
        }
      }
      return retVal;
    }

    public async Task<ProtoExecuteActionResponse> ExecuteActionGetResult(ProtoActionExe action)
    {
      if (action.Name == GetSnapshot)
      {
        return await HandleActionGetSnapshotAsync(action);
      }

      return new ProtoExecuteActionResponse()
      {
        Success = false,
        Message = "not implemented"
      };
    }
  }
}
