using Domain;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Itinero;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace LeafletAlarmsRouter
{
  public class TrackRouter: ITrackRouter
  {
    public TrackRouter(IOptions<RoutingSettings> routingSettings)
    {
      string dataLocation = routingSettings.Value.RoutingFilePath;
      Bootstrapper.BootFromConfiguration(dataLocation);
    }

    List<Coordinate> ConvertCoordsToNative(List<Geo2DCoordDTO> coords)
    {
      List<Coordinate> list_native = new List<Coordinate>();

      foreach (var coord in coords)
      {
        list_native.Add(new Coordinate()
        {
          Latitude = (float)coord.Y,
          Longitude = (float)coord.X
        });
      }
      return list_native;
    }

    List<Geo2DCoordDTO>  ConvertNativeToCoords(List<Coordinate> coords)
    {
      List<Geo2DCoordDTO> list_out = new List<Geo2DCoordDTO>();

      foreach (var coord in coords)
      {
        list_out.Add(new Geo2DCoordDTO()
        {
          (double)coord.Latitude,
          (double)coord.Longitude
        });
      }
      return list_out;
    }

    public bool IsMapExist(string inst)
    {
      if (InstanceManager.TryGet(inst, out var instance))
      {
        return true;
      }
      return false;
    }

    public async Task<List<Geo2DCoordDTO>> GetRoute(string inst, List<Geo2DCoordDTO> coords)
    {
      await Task.Delay(0);

      if (InstanceManager.TryGet(inst, out var instance))
      {
        // get a profile.
        var profile = Vehicle.Car.Fastest(); // the default OSM car profile.
        //var start = router.Resolve(profile, 51.51467784097949f, -0.1486710157204226f);
        //var end = router.Resolve(profile, 51.1237f, 1.3134f);
        List<Coordinate> list_native = ConvertCoordsToNative(coords);

        // calculate a route.
        

        try
        {
          var route = instance.Calculate(profile.FullName, list_native.ToArray());

          if (route.IsError)
          {
            Console.WriteLine($"Route error:{route.ErrorMessage}");
            return null;
          }
          return ConvertNativeToCoords(route.Value.Shape.ToList());
        }
        catch(Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }        
      }

      return null;
    }
  }
}