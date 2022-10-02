using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class RoutingSettings
  {
    private string _routingFilePath;
    public string RoutingFilePath { 
      get
      {
        if (!InDocker())
        {
          return RoutingFilePathWin;
        }
        return _routingFilePath;
      }
      set
      {
        _routingFilePath = value;
      }
    }
    public string RoutingFilePathWin { get; set; }
    public string RouteInstanse { get; set; }
  bool InDocker()
    {
      return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }
  }
}
