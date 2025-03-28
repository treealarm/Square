using System.Runtime.CompilerServices;

namespace GrpcDaprLib
{
  public class Logger
  {
    public static void LogException(Exception ex, string text = "", [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
      Console.WriteLine($"{text} {ex.Message} {caller} {file}");
    }
  }
}
