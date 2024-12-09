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
  }
}
