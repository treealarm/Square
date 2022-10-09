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
      _figures.circles = new List<FigureCircleDTO>();

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

    private async Task<List<Geo2DCoordDTO>> GetTestCoords(Geo2DCoordDTO from)
    {
      var coords = new List<Geo2DCoordDTO>();
      coords.Add(from);
      coords.Add(new Geo2DCoordDTO()
      { from.X, from.Y });

      double b = Math.PI + random.NextDouble() * 4;

      for (double rad = 0.05; rad < 1; rad += 0.01)
      {
        for (double a = b; a < (b + Math.PI * 2); a += 0.016)
        {
          var x = from.X + Math.Cos(a) * rad;
          var y = from.Y + Math.Sin(a) * rad;
          coords[1].X = x;
          coords[1].Y = y;

          //var ret = await _router.GetRoute("russia-latest", coords);

          //if (ret != null && ret.Count > 2)
          //{
          //  return ret;
          //}
        }
      }

      return coords;
    }

    double GetRandomDouble(double min, double max)
    {
      return min + (_random.NextDouble() * (max - min));
    }

    private async Task GetOrBuildFiguresAsync()
    {
      var figures = new FiguresDTO();
      figures.circles = new List<FigureCircleDTO>();

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

          var figure = new FigureCircleDTO()
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


    public async Task RunAsync()
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
          counter++;
          var coord = figure.geometry.coord;

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
  }
}
