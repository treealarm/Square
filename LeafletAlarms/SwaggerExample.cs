using Domain;
using Swashbuckle.AspNetCore.Filters;

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
