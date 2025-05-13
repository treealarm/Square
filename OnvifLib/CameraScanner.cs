using System.Net;

namespace OnvifLib
{
  public static class CameraScanner
  {
    public static async Task<List<Camera>> ScanAsync(
        string ipStart,
        string ipEnd,
        int port,
        List<(string username, string password)> credentials,
        int timeoutMs = 3000)
    {
      var cameras = new List<Camera>();
      var start = IPAddress.Parse(ipStart).GetAddressBytes();
      var end = IPAddress.Parse(ipEnd).GetAddressBytes();

      if (start.Length != 4 || end.Length != 4)
        throw new ArgumentException("Only IPv4 is supported.");

      uint ipStartInt = ToUInt32(start);
      uint ipEndInt = ToUInt32(end);

      var tasks = new List<Task>();

      for (uint ip = ipStartInt; ip <= ipEndInt; ip++)
      {
        var ipStr = new IPAddress(BitConverter.GetBytes(ip)).ToString();
        tasks.Add(Task.Run(async () =>
        {
          foreach (var (username, password) in credentials)
          {
            try
            {
              using var cts = new CancellationTokenSource(timeoutMs);
              var createTask = Camera.CreateAsync(ipStr, port, username, password);
              var camera = await createTask.WaitAsync(cts.Token);
              if (camera != null)
              {
                lock (cameras)
                  cameras.Add(camera);
                break;
              }
            }
            catch
            {
              // Игнорируем ошибки — скорее всего, либо нет камеры, либо неверные креды
            }
          }
        }));
      }

      await Task.WhenAll(tasks);
      return cameras;
    }

    private static uint ToUInt32(byte[] bytes)
    {
      if (BitConverter.IsLittleEndian)
        Array.Reverse(bytes);
      return BitConverter.ToUInt32(bytes, 0);
    }
  }

}
