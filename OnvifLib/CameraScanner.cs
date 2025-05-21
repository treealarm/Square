using OnvifLib;
using System.Collections.Concurrent;
using System.Net;
namespace OnvifLib
{
  public static class CameraScanner
  {
    public static async Task<bool> ScanAsync(
        string ipStart,
        string ipEnd,
        List<int> ports,
        List<(string username, string password)> credentials,
        Func<int, string, Task> onProgress,
        Func<Camera, Task> onCameraDiscovered,
        CancellationToken token,
        List<Camera> existing)
    {
      var startIp = IPAddress.Parse(ipStart);
      var endIp = IPAddress.Parse(ipEnd);
      var range = new IpRangeEnumerator(startIp, endIp);

      int total = range.Count() * credentials.Count * ports.Count;
      int step = 0;

      foreach (var port in ports)
      {
        foreach (var ip in range)
        {
          foreach (var cred in credentials)
          {
            token.ThrowIfCancellationRequested();

            int progress = (int)(step * 100.0 / total);
            step++;

            await onProgress(progress, "in progress");

            string url = Camera.CreateUrl(ip.ToString(), port);

            var existingCam = existing.FirstOrDefault(c => c.Url == url);
            if (existingCam != null && await existingCam.IsAlive())
            {
              continue;
            }

            var cam = await Camera.CreateAsync(ip.ToString(), port, cred.username, cred.password);
            if (cam == null || !await cam.IsAlive())
            {
              continue;
            }
            await onCameraDiscovered(cam);
          }
        }
      }

      await onProgress(100, "finished");
      return true;
    }
  }
}