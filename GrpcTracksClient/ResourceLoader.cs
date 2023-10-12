using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrpcTracksClient
{
  internal class ResourceLoader
  {
    static public async Task<string> GetResource(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();

      string s = string.Empty;
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      using (StreamReader reader = new StreamReader(stream))
      {
        s = await reader.ReadToEndAsync();
      }
      return s;
    }
  }
}
