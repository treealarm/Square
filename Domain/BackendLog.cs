using System;
using System.IO;

namespace Domain
{
  // Minimal console+file logging for catch blocks that otherwise swallow exceptions silently.
  // Not a replacement for ILogger — just enough to leave a trail on disk (logs/backend-errors.log
  // at the repo root) so failures can be inspected after the fact, without needing the
  // process's own console/Debug Console output.
  public static class BackendLog
  {
    private static readonly object _lock = new();
    private static readonly string _logPath = Path.Combine(
      AppContext.BaseDirectory, "..", "..", "..", "..", "logs", "backend-errors.log");

    public static void LogError(string source, Exception ex)
    {
      var line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}Z] {source}: {ex}";
      Console.WriteLine(line);

      try
      {
        lock (_lock)
        {
          var dir = Path.GetDirectoryName(_logPath)!;
          Directory.CreateDirectory(dir);
          File.AppendAllText(_logPath, line + Environment.NewLine + Environment.NewLine);
        }
      }
      catch
      {
        // Logging must never be the reason a request fails.
      }
    }
  }
}
