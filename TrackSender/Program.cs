using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Itinero;
using Itinero.Osm.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TrackSender
{
  class Program
  {
    static HttpClient client = new HttpClient();

    static void Main(string[] args)
    {
      try
      {
        RunAsync().GetAwaiter().GetResult();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    static async Task<FiguresDTO> UpdateFiguresAsync(FiguresDTO figure, string action)
    {
      HttpResponseMessage response = 
        await client.PostAsJsonAsync(
          $"api/Tracks/{action}", figure);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      try
      {
        FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
        return json;
      }
      catch(Exception ex)
      {

      }
      
      return figure;
    }

    static async Task<List<BaseMarkerDTO>> GetByName(string name)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Map/GetByName", name);
      
      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      var json = JsonSerializer.Deserialize<List<BaseMarkerDTO>>(s);
      return json;
    }

    static async Task<FiguresDTO> GetByIds(List<string> ids)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Map/GetByIds", ids);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
      return json;
    }

    static async Task<string> Empty(string ids)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Tracks/Empty", ids);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      return s;
    }

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
      //var end = router.Resolve(profile, 51.54685922108974f, -0.07535808869446825f);
      var end = router.Resolve(profile, 51.1237f, 1.3134f);


      // calculate a route.
      var route = router.Calculate(profile, start, end);

      return route.Shape;
    }
    static async Task RunAsync()
    {
      var root_coords = await RunRouteAsync();
      // Update port # in the following line.
      client.BaseAddress = new Uri("https://localhost:44307/");
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));


      {
        // Create Route.
        var test_route = await GetByName("test_route");
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
          polylines = await GetByIds(new List<string>() { test_route.FirstOrDefault().id });
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
        polylines = await UpdateFiguresAsync(polylines, "AddTracks");
      }


      var markers = await GetByName("test_track");
      FiguresDTO figures;
      FigureCircleDTO figure;

      if (markers == null || markers.Count == 0)
      {
        figures = new FiguresDTO();
        figures.circles = new List<FigureCircleDTO>();
        figure = new FigureCircleDTO()
        {
          name = "test_track",
          radius = 333,
          zoom_level = "13"
        };
        figures.circles.Add(figure);

        figure.geometry = new GeometryCircleDTO(new Geo2DCoordDTO() { 51.512677840979485, -0.14968839124598346 });
        figures = await UpdateFiguresAsync(figures, "AddTracks");
      }
      else
      {
        figures = await GetByIds(new List<string>() { markers.FirstOrDefault().id });
      }
      
      //var t1 = DateTime.Now;

      //for (int i = 0; i< 100000; i++)
      //{
      //  var sfigures = await Empty("62b85c84736cb579f3154917");

      //  if (i%2000 == 0)
      //  {
      //    var t3 = (DateTime.Now - t1).TotalSeconds;
      //    Console.WriteLine($"{i}-{t3}");
      //    t1 = DateTime.Now;
      //  }
      //}

      //var t2 = (DateTime.Now - t1).TotalSeconds;

      figure = figures.circles.FirstOrDefault();

      if (figure == null)
      {
        return;
      }

      figure.geometry = new GeometryCircleDTO(new Geo2DCoordDTO() { 51.512677840979485, -0.14968839124598346 }) ;
      var stat_y = figure.geometry.coord[0];
      var stat_x = figure.geometry.coord[1];
      Random rand = new Random();

      for (double dy = 0; dy < 0.1; dy+=0.001)
      {
        var dx = rand.NextDouble() / 500;
        figure.geometry.coord[0] = stat_y + dy;
        figure.geometry.coord[1] = stat_x + dx;
        figure.zoom_level = "13";
        Console.WriteLine(JsonSerializer.Serialize(figure.geometry));
        await UpdateFiguresAsync(figures, "UpdateTracks");
        await Task.Delay(1000);
      }      
    }
  }
}
