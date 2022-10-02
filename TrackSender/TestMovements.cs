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

namespace TrackSender
{
  internal class TestMovements
  {
    TestClient _testClient = new TestClient();
    TrackRouter _router;

    public TestMovements()
    {
      var settings = new RoutingSettings()
      {
        RoutingFilePath = "D:\\TESTS\\OSM_DATA\\",
        RoutingFilePathWin = "D:\\TESTS\\OSM_DATA\\"
      };
      var appSettingsOptions = Options.Create(settings);

      _router = new TrackRouter(appSettingsOptions);

      while (!_router.IsMapExist("russia-latest"))
      {
        Task.Delay(10);
      }
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

          var ret = await _router.GetRoute("russia-latest", coords);

          if (ret != null && ret.Count > 2)
          {
            return ret;
          }
        }
      }

      return coords;
    }
    public async Task RunAsync()
    {
      var start =
        new GeometryCircleDTO(new Geo2DCoordDTO() { 55.977606, 37.186745 });


      FiguresDTO figures;
      figures = await _testClient.GetByParams("track_name", "lisa_alert");

      if (figures == null || figures.circles.Count == 0)
      {
        figures = new FiguresDTO();
        figures.circles = new List<FigureCircleDTO>();

        for (int i = 0; i < 10; i++)
        {
          var extra_props = new List<ObjExtraPropertyDTO>()
          {
            new ObjExtraPropertyDTO()
            {
              prop_name = "track_name",
              str_val = "lisa_alert"
            },
            new ObjExtraPropertyDTO()
            {
              prop_name = "timestamp",
              str_val = DateTime.UtcNow
                .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            }
          };

          var figure = new FigureCircleDTO()
          {
            name = $"test_track{i}",
            radius = 222,
            zoom_level = "13",
            geometry = start,
            extra_props = extra_props
          };
          figures.circles.Add(figure);
        }

        figures = await _testClient.UpdateFiguresAsync(figures, "AddTracks");
      }

      //55.872425, 37.456428 -> 55.621026, 37.786127

      var x_start = 37.456428;
      var x_end = 37.786127;
      var x_step = (x_end - x_start) / 10;

      var y_start = 55.621026;
      var y_end = 55.872425;
      var y_step = (y_end - y_start) / 10;

      int counter = 0;

      for (double x = x_start; x < x_end; x += x_step)
      {
        
        double y = y_start;
        {
          foreach (var figure in figures.circles)
          {
            counter++;
            var coord = new Geo2DCoordDTO() { y, x };
            //var rout = await GetTestCoords(figure.geometry.coord);
            var extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "track_name",
                str_val = "lisa_alert"
              },
              new ObjExtraPropertyDTO()
              {
                prop_name = "timestamp",
                str_val = (DateTime.UtcNow
                + new TimeSpan(0,0,counter, 0)
                )
              .ToString("o", System.Globalization.CultureInfo.InvariantCulture)
              }
            };
            figure.geometry.coord = coord;
            figure.extra_props = extra_props;
            y += y_step;
          }

          //await Task.Delay(2000);
          await _testClient.UpdateFiguresAsync(figures, "UpdateTracks");
          Console.WriteLine(counter.ToString());
        }
      }
    }
  }
}
