using Itinero;
using System;
using System.Collections.Generic;
using System.IO;
using Itinero.Osm.Vehicles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain;

namespace TrackSender
{
  internal class RoutTester
  {
    static async Task<Itinero.LocalGeo.Coordinate[]> RunRouteAsync()
    {
      await Task.Delay(1);
      var routerDb = new RouterDb();

      try
      {
        var file2load = @"D:\TESTS\OSM_DATA\great-britain.routerdb";
        //var file2load = @"D:\TESTS\OSM_DATA\great-britain-latest.osm.pbf";

        using (var stream = new FileInfo(file2load).OpenRead())
        {
          routerDb = RouterDb.Deserialize(stream);

          //routerDb.LoadOsmData(stream, Vehicle.Car); // create the network for cars only.
        }

        //using (var stream = new FileInfo(@"D:\TESTS\OSM_DATA\file.routerdb").Open(FileMode.Create))
        //{
        //routerDb.AddContracted(routerDb.GetSupportedProfile("car"));
        //  routerDb.Serialize(stream);
        //}

      }
      catch (Exception ex)
      {

      }

      var edges = routerDb.GetGeoJsonEdges();

      // create a router.
      var router = new Router(routerDb);

      // get a profile.
      var profile = Vehicle.Car.Fastest(); // the default OSM car profile.

      // create a routerpoint from a location.
      // snaps the given location to the nearest routable edge.
      var start = router.Resolve(profile, 51.51467784097949f, -0.1486710157204226f);
      var end = router.Resolve(profile, 51.1237f, 1.3134f);


      // calculate a route.
      var route = router.Calculate(profile, start, end);

      return route.Shape;
    }

    static async Task RunAsync()
    {
      var root_coords = await RunRouteAsync();

      if (root_coords != null)
      {
        // Create Route.
        var test_route = await TestClient.GetByName("test_route");
        FigurePolylineDTO polyline;
        var polylines = new FiguresDTO();

        if (test_route == null || test_route.Count == 0)
        {

          polylines.polylines = new List<FigurePolylineDTO>();

          polyline = new FigurePolylineDTO()
          {
            name = "test_route",
            zoom_level = "13"
          };


          polyline.geometry = new GeometryPolylineDTO();

          polylines.polylines.Add(polyline);

        }
        else
        {
          polylines = await TestClient.GetByIds(new List<string>() { test_route.FirstOrDefault().id });
        }

        polyline = polylines.polylines.FirstOrDefault();
        polyline.geometry.coord.Clear();

        foreach (var coord in root_coords)
        {
          polyline.geometry.coord.Add(new Geo2DCoordDTO()
            {
              coord.Latitude,
              coord.Longitude
            });
        }
        polylines = await TestClient.UpdateFiguresAsync(polylines, "AddTracks");
      }
    }
  }
}
