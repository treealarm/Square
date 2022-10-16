using Domain.GeoDBDTO;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Itinero.Profiles;
using Itinero;
using System.Reflection;
using LeafletAlarmsRouter;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Threading;
using Domain.GeoDTO;

namespace TrackSender
{
  internal class TestMovements
  {
    TestClient _testClient = new TestClient();
    //TrackRouter _router;
    FiguresDTO _figures = new FiguresDTO();
    Random _random = new Random();   

    public TestMovements()
    {
      _figures.circles = new List<FigureGeoDTO>();

      //var settings = new RoutingSettings()
      //{
      //  RoutingFilePath = "D:\\TESTS\\OSM_DATA\\",
      //  RoutingFilePathWin = "D:\\TESTS\\OSM_DATA\\"
      //};
      //var appSettingsOptions = Options.Create(settings);

      //_router = new TrackRouter(appSettingsOptions);

      //while (!_router.IsMapExist("russia-latest"))
      //{
      //  Task.Delay(10);
      //}
    }

    private static Random random = new Random();

    double GetRandomDouble(double min, double max)
    {
      return min + (_random.NextDouble() * (max - min));
    }

    public async Task RunAsync(CancellationToken token, List<Task> tasks)
    {      
      var task = GetOrBuildFiguresOnMkadAsync(token, tasks);
      tasks.Add(task);
    }

    private async Task GetOrBuildFiguresAsync()
    {
      var figures = new FiguresDTO();
      figures.circles = new List<FigureGeoDTO>();

      List<string> existedIds = new List<string>();
      for (int i = 0; i < 100; i++)
      {
        var obj_name = $"test_track{i}";
        Console.WriteLine("Getting:" + obj_name);
        var marker = await _testClient.GetByName(obj_name);

        if (marker != null && marker.Count > 0)
        {
          existedIds.AddRange(marker.Select(m => m.id));
        }
        else
        {
          var extra_props = new List<ObjExtraPropertyDTO>()
          {
            new ObjExtraPropertyDTO()
            {
              prop_name = "track_name",
              str_val = $"lisa_alert"
            },
            new ObjExtraPropertyDTO()
            {
              prop_name = "timestamp",
              str_val = DateTime.UtcNow
                .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            }
          };

          //55.872425, 37.456428 -> 55.621026, 37.786127

          var y = GetRandomDouble(55.872425, 55.621026);
          var x = GetRandomDouble(37.456428, 37.786127);
          var start =
              new GeometryCircleDTO(
                new Geo2DCoordDTO() { y, x }
                );

          var figure = new FigureGeoDTO()
          {
            name = obj_name,
            radius = 222,
            zoom_level = "13",
            geometry = start,
            extra_props = extra_props
          };
          figures.circles.Add(figure);
        }
      }

      if (figures != null && figures.circles.Count > 0)
      {
        figures = await _testClient.UpdateTracksAsync(figures, "AddTracks");
        _figures.circles.AddRange(figures.circles);
      }

      if (existedIds.Count > 0)
      {
        figures = await _testClient.GetByIds(existedIds);
        _figures.circles.AddRange(figures.circles);
      }
    }

    private async Task GetOrBuildFiguresOnMkadAsync(CancellationToken token, List<Task> tasks)
    {
      var tempFigure = await NominatimProcessor.GetMkadPolyline();

      if (tempFigure == null)
      {
        return;
      }
      FigureGeoDTO mkadPolyline = null;

      mkadPolyline = tempFigure.FirstOrDefault();

      if (mkadPolyline ==  null)
      {
        return;
      }

      Random random = new Random();
      var geometry_mkad = mkadPolyline.geometry as GeometryPolylineDTO;

      int maxPoint = geometry_mkad.coord.Count;

      
      var max_circles = 10000;
      var start_circles = 0;
      var figures = await _testClient.GetByParams("track_name", "mkad", max_circles);

      if (figures == null || figures.circles == null || figures.circles.Count < max_circles)
      {
        if (figures != null && figures.circles != null)
        {
          start_circles = figures.circles.Count;
        }

        figures = new FiguresDTO();
        figures.circles = new List<FigureGeoDTO>();

        for (int i = start_circles; i < max_circles; i++)
        {
          var color =
            $"#{_random.Next(20).ToString("X2")}{_random.Next(256).ToString("X2")}{_random.Next(256).ToString("X2")}";

          var obj_name = $"test_track{i}";
          var rnd = random.Next(0, maxPoint);

          var extra_props = new List<ObjExtraPropertyDTO>()
          {
            new ObjExtraPropertyDTO()
            {
              prop_name = "track_name",
              str_val = $"mkad"
            },
            new ObjExtraPropertyDTO()
            {
              prop_name = "timestamp",
              str_val = DateTime.UtcNow
                .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            }
            ,
              new ObjExtraPropertyDTO()
              {
                prop_name = "color",
                str_val = color
              }
          };
          
          var y = geometry_mkad.coord[rnd].Y;
          var x = geometry_mkad.coord[rnd].X;
          var start =
                new GeometryCircleDTO(
                  new Geo2DCoordDTO() { y, x }
                  );

            var figure = new FigureGeoDTO()
            {
              name = obj_name,
              radius = 150,
              zoom_level = "13",
              geometry = start,
              extra_props = extra_props
            };
            figures.circles.Add(figure);
          }
        figures = await _testClient.UpdateTracksAsync(figures, "AddTracks");
      }

      figures = await _testClient.GetByParams("track_name", "mkad", max_circles);

      if (figures == null || figures.circles.Count == 0)
      {
        return;
      }

      Dictionary<string, CircleShifter> dicShifter = new Dictionary<string, CircleShifter>();

      foreach (var figure in figures.circles)
      {
        var geometry = figure.geometry as GeometryCircleDTO;

        var pointOnMkad = geometry_mkad.coord
          .Where(c => (c.X - geometry.coord.X) < 0.0001 && (c.Y - geometry.coord.Y) < 0.0001)
          .FirstOrDefault();

        if (pointOnMkad == null)
        {
          dicShifter[figure.id] = new CircleShifter()
          {
            CurIndex = random.Next(0, maxPoint)            
          };
        }
        else 
        {
          dicShifter[figure.id] = new CircleShifter()
          {
            CurIndex = geometry_mkad.coord.IndexOf(pointOnMkad)            
          };
        }

        dicShifter[figure.id].ShiftSteps = random.Next(-2, 3);

        if (dicShifter[figure.id].ShiftSteps == 0)
        {
          dicShifter[figure.id].ShiftSteps = 1;
        }
      }

      int counter = 0;

      while (!token.IsCancellationRequested)
      {
        foreach (var figure in figures.circles)
        {
          var geometry = figure.geometry as GeometryCircleDTO;
          counter++;

          var shifter = dicShifter[figure.id];

          shifter.CurIndex += shifter.ShiftSteps;

          if (shifter.CurIndex < 0)
          {
            shifter.CurIndex = maxPoint - 1;
          }
          else if(shifter.CurIndex >= maxPoint)
          {
            shifter.CurIndex = 0;
          }

          var y = geometry_mkad.coord[shifter.CurIndex].Y;
          var x = geometry_mkad.coord[shifter.CurIndex].X;
          
          var start =
                new GeometryCircleDTO(
                  new Geo2DCoordDTO() { y, x }
                  );
          if (Math.Abs(geometry.coord.X - x) < 0.0000001)
          {
            start.coord.X = x + 0.0000001;
          }

          figure.geometry = start;

          var extra_props = new List<ObjExtraPropertyDTO>() {
            new ObjExtraPropertyDTO()
            {
              prop_name = "timestamp",
              str_val = (DateTime.UtcNow
              + new TimeSpan(0, 0, 0, counter)
              )
            .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            }
          };

          figure.extra_props = extra_props;
        }

        await _testClient.UpdateTracksAsync(figures, "UpdateTracks");
      }
    }

    async Task BuildRandomFigures()
    {
      await GetOrBuildFiguresAsync();

      var start =
        new GeometryCircleDTO(new Geo2DCoordDTO() { 55.977606, 37.186745 });

      //55.872425, 37.456428 -> 55.621026, 37.786127


      int counter = 0;
      int steps = 100;

      for (int j = 0; j < steps; j++)
      {
        foreach (var figure in _figures.circles)
        {
          var geometry = figure.geometry as GeometryCircleDTO;
          counter++;
          var coord = geometry.coord;

          coord.X = GetRandomDouble(coord.X - 0.01, coord.X + 0.01);
          coord.Y = GetRandomDouble(coord.Y - 0.01, coord.Y + 0.01);
          //var rout = await GetTestCoords(figure.geometry.coord);
          var extra_props = new List<ObjExtraPropertyDTO>() {
            new ObjExtraPropertyDTO()
            {
              prop_name = "timestamp",
              str_val = (DateTime.UtcNow
              + new TimeSpan(0,0,counter, 0)
              )
            .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            }
          };

          figure.extra_props = extra_props;
        }

        //await Task.Delay(2000);
        await _testClient.UpdateTracksAsync(_figures, "UpdateTracks");
        Console.WriteLine(j.ToString());
      }
    }


    public async Task CreateMkad()
    {
      var mkad = await _testClient.GetByParams("mkad", "true", 1);

      if (mkad == null || mkad.IsEmpty())
      {
        mkad = new FiguresDTO();
        mkad.polylines = await NominatimProcessor.GetMkadPolyline();
        mkad = await _testClient.UpdateFiguresAsync(mkad);
      }
    }
  }
}
