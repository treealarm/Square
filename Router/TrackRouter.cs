using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
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
      string dataLocation = Path.Combine(routingSettings.Value.RootFolder, "osm_data");
      Bootstrapper.BootFromConfiguration(dataLocation);
    }

    List<Coordinate> ConvertCoordsToNative(List<Geo2DCoordDTO> coords)
    {
      List<Coordinate> list_native = new List<Coordinate>();

      foreach (var coord in coords)
      {
        list_native.Add(ConvertCoordToNative(coord));
      }
      return list_native;
    }

    Coordinate ConvertCoordToNative(Geo2DCoordDTO coord)
    {
     return new Coordinate()
        {
          Latitude = (float)coord.Y,
          Longitude = (float)coord.X
        };      
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

    public void RemoveEdges(string inst, string profileName, HashSet<uint> toRemove)
    {
      if (InstanceManager.TryGet(inst, out var instance))
      {
        instance.RemoveEdges(profileName, toRemove);
      }
    }

    public bool IsMapExist(string inst)
    {
      if (InstanceManager.TryGet(inst, out var instance))
      {
        return true;
      }
      return false;
    }

    private Itinero.Profiles.Profile GetProfile(string strProfile)
    {
      // get a profile.
      var profile = Vehicle.Car.Fastest(); // the default OSM car profile.

      if (strProfile.ToLower() == "bicycle")
      {
        profile = Vehicle.Bicycle.Fastest();
      }

      if (strProfile.ToLower() == "pedestrian")
      {
        profile = Vehicle.Pedestrian.Fastest();
      }

      if (strProfile.ToLower() == "moped")
      {
        profile = Vehicle.Moped.Fastest();
      }

      return profile;
    }
    public  List<TreeEdgeDTO> CalculateTree(
      string inst,
      string strProfile,
      Geo2DCoordDTO coord,
      int max
    )
    {
      if (InstanceManager.TryGet(inst, out var instance))
      {
        var retVal = new List<TreeEdgeDTO>();
        var profile = GetProfile(strProfile);
        var native = ConvertCoordToNative(coord);
        var result = instance.CalculateTree(profile.FullName, native, max);

        foreach (var item in result.Value.Edges)
        {
          retVal.Add(new TreeEdgeDTO()
          {
            Shape = new Geo2DCoordDTO()
            {
              Lat = item.Shape[0][1],
              Lon = item.Shape[0][0]
            },
            EdgeId = item.EdgeId,
            PreviousEdgeId = item.PreviousEdgeId,
            Weight1 = item.Weight1,
            Weight2 = item.Weight2
          });
        }

        return retVal;
      }

      return null;
    }
    public async Task<List<Geo2DCoordDTO>> GetRoute(
      string inst,
      string strProfile,
      List<Geo2DCoordDTO> coords
    )
    {
      await Task.Delay(0);

      if (InstanceManager.TryGet(inst, out var instance))
      {
       
        //var start = router.Resolve(profile, 51.51467784097949f, -0.1486710157204226f);
        //var end = router.Resolve(profile, 51.1237f, 1.3134f);
        List<Coordinate> list_native = ConvertCoordsToNative(coords);
        List<Coordinate> f_native = null;
        // calculate a route.
        var profile = GetProfile(strProfile);

        try
        {
          var route = instance.Calculate(
            profile.FullName,
            list_native
          );

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