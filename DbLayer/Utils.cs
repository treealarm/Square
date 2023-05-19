using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public class Utils
  {
    static public string GenerateBsonId()
    {
      return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
    }
  }
}
