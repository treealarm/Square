namespace IntegrationUtilsLib
{
  public class Utils
  {
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
          Logger.LogException(ex, $"taskName={taskName}");
          await Task.Delay(5000, token);
        }
      }
    }
  }
}
