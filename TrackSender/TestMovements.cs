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
        RoutingFilePath = "D:\\TESTS\\OSM_DATA\\"
      };
      var appSettingsOptions = Options.Create(settings);

      _router = new TrackRouter(appSettingsOptions);

      while (!_router.IsMapExist(String.Empty))
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

      double b = Math.PI +random.NextDouble() * 4;

      for (double rad = 0.05; rad < 1; rad += 0.01)
      {
        for (double a = b; a < (b + Math.PI * 2); a += 0.016)
        {
          var x = from.X + Math.Cos(a)* rad;
          var y = from.Y + Math.Sin(a)* rad;
          coords[1].X = x;
          coords[1].Y = y;

          var ret = await _router.GetRoute(string.Empty, coords);

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
        new GeometryCircleDTO(new Geo2DCoordDTO() { 51.51467784097949, -0.1486710157204226 });

      var extra_props = new List<ObjExtraPropertyDTO>()
        {
          new ObjExtraPropertyDTO()
          {
            prop_name = "track_name",
            str_val = "lisa_alert"
          }
        };

      FiguresDTO figures;
      figures = await _testClient.GetByParams("track_name", "lisa_alert");

      if (figures == null || figures.circles.Count == 0)
      {
        figures = new FiguresDTO();
        figures.circles = new List<FigureCircleDTO>();

        for (int i = 0; i < 10; i++)
        {
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

      for(int i = 0; i < 20; i++)
      {
        foreach (var figure in figures.circles)
        {
          var rout = await GetTestCoords(figure.geometry.coord);
          figure.geometry.coord = rout.Last();
          figure.extra_props = extra_props;
        }
        await Task.Delay(2000);
        await _testClient.UpdateFiguresAsync(figures, "UpdateTracks");
        Console.WriteLine(i.ToString());
      }
    }
  }
}
