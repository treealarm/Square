using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcTracksClient
{
  internal class Utils
  {
    public static string LongTo24String(long number)
    {
      return "1111" + number.ToString("D20");
    }
    public static async Task RunTaskWithRetry(Func<Task> taskFunc, string taskName, CancellationToken token)
    {
      while (!token.IsCancellationRequested)
      {
        try
        {
          await taskFunc();
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
          await Task.Delay(1000, token); // Задержка перед повторной попыткой
        }
      }
    }
  }
}
