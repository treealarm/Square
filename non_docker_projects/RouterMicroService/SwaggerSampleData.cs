using Domain;
using Domain.GeoDBDTO;
using Domain.StateWebSock;
using Swashbuckle.AspNetCore.Filters;

namespace RouterMicroService
{
  public class BicycleRoutExample : IExamplesProvider<RoutDTO>
  {
    public RoutDTO GetExamples()
    {
      return new RoutDTO
      {
        Profile = @"bicycle",
        InstanceName = @"RU-MOS",
        Coordinates = new List<Geo2DCoordDTO>()
        {
          new Geo2DCoordDTO()
          {
            Lat = 55.750210, Lon = 37.624015
          },
          new Geo2DCoordDTO()
          {
            Lat = 55.756489, Lon = 37.615216
          }
        }
      };
    }
  }
}
