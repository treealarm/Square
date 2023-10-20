using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValhallaLib
{
  public class Auto
  {
    public double country_crossing_penalty { get; set; } = 2000.0;
  }

  public class CostingOptions
  {
    public Auto? auto { get; set; } = new Auto();
  }

  public class LocationRequest
  {
    public double lat { get; set; }
    public double lon { get; set; }
  }

  public class Root
  {
    public List<LocationRequest> locations { get; set; } = new List<LocationRequest>();
    public string? costing { get; set; } = "auto";
    public CostingOptions? costing_options { get; set; } = new CostingOptions();
    public string? id { get; set; } = "valhalla";
  }
}
