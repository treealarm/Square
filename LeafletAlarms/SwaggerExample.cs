using Domain;
using Domain.StateWebSock;
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

  public class RoutDTOExample : IExamplesProvider<RoutDTO>
  {
    public RoutDTO GetExamples()
    {
      return new RoutDTO
      {
        InstanceName = @"great-britain",
        Coordinates = new List<Domain.GeoDBDTO.Geo2DCoordDTO>()
        {
          new Domain.GeoDBDTO.Geo2DCoordDTO()
          {
            51.51467784097949, -0.1486710157204226
          },
          new Domain.GeoDBDTO.Geo2DCoordDTO()
          {
            51.1237, 1.3134
          }
        }
      };
    }
  }

  public class StaticLogicDTOExample : IExamplesProvider<StaticLogicDTO>
  {
    public StaticLogicDTO GetExamples()
    {
      return new StaticLogicDTO
      {
        logic = @"from-to",
        figs= new List<List<string>>()
        {
          new List<string>()
          {
            "63712210d3461ffd39aae4c6"
          },
          new List<string>()
          {
            "6373a695ca6b610ed0884a94"
          }
        }
      };
    }
  }
}
