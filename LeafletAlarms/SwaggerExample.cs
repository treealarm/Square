using DbLayer;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms
{
  public class SwaggerExample
  {
  }
  public class SingleCircle : IExamplesProvider<CircleMarker>
  {
    public CircleMarker GetExamples()
    {
      return new CircleMarker
      {
        MarkerName = @"Test",
        Center = new double[] {51.505, -0.09 }
      };
    }
  }
}
