using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrpcTracksClient
{
  internal class Logger
  {
    public static void LogException(Exception ex, string text = "", [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
      Console.WriteLine($"{text} {ex.Message} {caller} {file}");
    }
  }
}
