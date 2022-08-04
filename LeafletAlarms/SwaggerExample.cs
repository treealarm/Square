using Domain;
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
  public class SingleCircle : IExamplesProvider<BaseMarkerDTO>
  {
    public BaseMarkerDTO GetExamples()
    {
      return new BaseMarkerDTO
      {
        name = @"Test",
        parent_id = null
      };
    }
  }
}
