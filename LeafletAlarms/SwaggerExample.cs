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
  public class SingleCircle : IExamplesProvider<Marker>
  {
    public Marker GetExamples()
    {
      return new Marker
      {
        name = @"Test",
        parent_id = null
      };
    }
  }
}
