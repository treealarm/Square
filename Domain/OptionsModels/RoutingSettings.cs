using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class RoutingSettings
  {
    public string RootFolder { 
      get
      {
        if (!InDocker())
        {
          return root_folder_win;
        }
        return root_folder;
      }
    }
    public string root_folder_win { get; set; } = default!;
    public string RouteInstanse { get; set; } = default!;
    public string root_folder { get; set; } = default!;
    bool InDocker()
    {
      return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }
  }
}
