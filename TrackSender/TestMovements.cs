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
using System.Net.Http;

namespace TrackSender
{
  internal class TestMovements
  {
    TestClient _testClient;
    //TrackRouter _router;
    FiguresDTO _figures = new FiguresDTO();
    Random _random = new Random();

    public TestMovements(HttpClient client)
    {
      _testClient = new TestClient(client);
      _figures.figs = new List<FigureGeoDTO>();

      //var settings = new RoutingSettings()
      //{
      //  RoutingFilePath = "D:\\TESTS\\LEAFLET_DATA\\OSM_DATA\\",
      //  RoutingFilePathWin = "D:\\TESTSLEAFLET_DATA\\\\OSM_DATA\\"
      //};
      //var appSettingsOptions = Options.Create(settings);

      //_router = new TrackRouter(appSettingsOptions);

      //while (!_router.IsMapExist("russia-latest"))
      //{
      //  Task.Delay(10);
      //}
    }

    double GetRandomDouble(double min, double max)
    {
      return min + (_random.NextDouble() * (max - min));
    }

    public async Task RunAsync(CancellationToken token, List<Task> tasks)
    {
      //await CreateRoad($"CKAD");
      var task = GetOrBuildFiguresOnRoadAsync("MKAD", token, 10000);
      tasks.Add(task);

      var task1 = GetOrBuildFiguresOnRoadAsync("CKAD", token, 1000000);
      tasks.Add(task1);
    }

    private async Task BuildMachines(string resFolder,
      GeometryPolylineDTO geometry_road,
      int maxMachines,
      string zoom_level
    )
    {
      int maxPoint = geometry_road.coord.Count;

      var max_circles = 10000;

      string start_id = string.Empty;

      var figures = await _testClient.GetByParams("track_name", resFolder, start_id, max_circles);

      int rnd = 0;

      if (figures == null || figures.figs == null || figures.IsEmpty())
      {
        for (int m = 0; m < maxMachines; m+= max_circles)
        {
          figures = new FiguresDTO();
          figures.figs = new List<FigureGeoDTO>();

          for (int i = 0; i < Math.Min(max_circles, maxMachines); i++)
          {
            var color =
              $"#{_random.Next(20).ToString("X2")}{_random.Next(256).ToString("X2")}{_random.Next(100).ToString("X2")}";

            var obj_name = $"{resFolder}{i}";
            rnd++;

            if (rnd >= maxPoint)
            {
              rnd = 0;
            }

            var extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "track_name",
                str_val = $"{resFolder}"
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

            var y = geometry_road.coord[rnd].Y;
            var x = geometry_road.coord[rnd].X;

            var start =
                  new GeometryCircleDTO(
                    new Geo2DCoordDTO() { y, x }
                    );

            var figure = new FigureGeoDTO()
            {
              name = obj_name,
              radius = 50,
              zoom_level = i == 0 ? "" : zoom_level,
              geometry = start,
              extra_props = extra_props
            };
            figures.figs.Add(figure);
          }
          figures = await _testClient.UpdateTracksAsync(figures, "AddTracks");
        }        
      }
    }
    private async Task GetOrBuildFiguresOnRoadAsync(
      string resFolder,
      CancellationToken token,
      int maxMachines,
      string zoom_level = "13"
    )
    {
      var tempFigure = await NominatimProcessor.GetRoadPolyline(resFolder);

      if (tempFigure == null)
      {
        return;
      }
      FigureGeoDTO mkadPolyline = null;

      mkadPolyline = tempFigure.FirstOrDefault();

      if (mkadPolyline == null)
      {
        return;
      }

      var geometry_mkad = mkadPolyline.geometry as GeometryPolylineDTO;
      int maxPoint = geometry_mkad.coord.Count;


      var max_circles = 10000;
      string start_id = string.Empty;

      await BuildMachines(resFolder, geometry_mkad, maxMachines, zoom_level);

      int counter = 0;
      Dictionary<string, CircleShifter> dicShifter = new Dictionary<string, CircleShifter>();

      while (!token.IsCancellationRequested)
      {
        await Task.Delay(100);

        var figures = await _testClient.GetByParams("track_name", $"{resFolder}", start_id, max_circles);

        if (figures == null || figures.figs.Count == 0)
        {
          start_id = string.Empty;
          continue;
        }

        start_id = figures.figs.LastOrDefault().id;

        

        foreach (var figure in figures.figs)
        {
          var geometry = figure.geometry as GeometryCircleDTO;

          if (!dicShifter.ContainsKey(figure.id))
          {
            var pointOnMkad = geometry_mkad.coord
              .Where(c => Math.Abs(c.X - geometry.coord.X) < 0.00001
              && Math.Abs(c.Y - geometry.coord.Y) < 0.00001)
              .FirstOrDefault();

            if (pointOnMkad == null)
            {
              dicShifter[figure.id] = new CircleShifter()
              {
                CurIndex = _random.Next(0, maxPoint)
              };
            }
            else
            {
              dicShifter[figure.id] = new CircleShifter()
              {
                CurIndex = geometry_mkad.coord.IndexOf(pointOnMkad)
              };
            }

            dicShifter[figure.id].ShiftSteps = _random.Next(-2, 3);

            if (dicShifter[figure.id].ShiftSteps == 0)
            {
              dicShifter[figure.id].ShiftSteps = 1;
            }
          }
        }


        foreach (var figure in figures.figs)
        {
          var geometry = figure.geometry as GeometryCircleDTO;
          counter++;

          var shifter = dicShifter[figure.id];

          shifter.CurIndex += shifter.ShiftSteps;

          if (shifter.CurIndex < 0)
          {
            shifter.CurIndex = 0;
            shifter.ShiftSteps *= -1;
          }
          else if (shifter.CurIndex >= maxPoint)
          {
            shifter.CurIndex = maxPoint - 1;
            shifter.ShiftSteps *= -1;
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
              //+ new TimeSpan(0, 0, 0, counter)
              )
            .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            }
          };

          figure.extra_props = extra_props;
        }

        Console.WriteLine($"UpdateTracks:{resFolder}");
        await Task.Delay(1000);
        await _testClient.UpdateTracksAsync(figures, "UpdateTracks");
      }
    }

    public async Task CreateRoad(string xmlFolderName)
    {
      var mkad = await _testClient.GetByParams(xmlFolderName, "true", string.Empty, 1);

      if (mkad == null || mkad.IsEmpty())
      {
        mkad = new FiguresDTO();
        mkad.figs = await NominatimProcessor.GetRoadPolyline(xmlFolderName);
        mkad = await _testClient.UpdateFiguresAsync(mkad);
      }
    }

    public void TestLogicAsync(CancellationToken token, List<Task> tasks)
    {
      var task1 = GetOrBuildFiguresOnRoadAsync("SAD", token, 10, "");
      tasks.Add(task1);
    }
  }
}
