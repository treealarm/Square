using OnvifLib;
using System.Collections.Concurrent;

namespace AASubService.Services
{
  public class CameraEventServiceManager
  {
    private readonly ConcurrentDictionary<string, EventService1> _activeServices = new();

    public async Task AddCameraAsync(string cameraId, Camera camera)
    {
      if (_activeServices.ContainsKey(cameraId))
        return;

      try
      {
        var service = await camera.GetEventService();
        if (service == null)
        {
          Console.WriteLine($"[{cameraId}] Event service is null");
          return;
        }

        service.OnEventReceived += msg =>
        {
          Console.WriteLine($"[{cameraId}] Event received");
          // логика обработки
        };

        await service.StartReceiving();

        if (!_activeServices.TryAdd(cameraId, service))
        {
          service.StopReceiving();
          service.Dispose();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[{cameraId}] Failed to start event service: {ex.Message}");
      }
    }


    public void RemoveCamera(string cameraId)
    {
      if (_activeServices.TryRemove(cameraId, out var service))
      {
        Console.WriteLine($"[{cameraId}] Stopping event service...");
        service.StopReceiving();
        service.Dispose();
      }
    }

    public void StopAll()
    {
      foreach (var (cameraId, service) in _activeServices)
      {
        service.StopReceiving();
        service.Dispose();
      }

      _activeServices.Clear();
    }
  }

}
