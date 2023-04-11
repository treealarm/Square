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
using Domain.StateWebSock;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.IO;

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

    public void RunAsync(CancellationToken token, List<Task> tasks)
    {
      //await CreateRoad($"CKAD");
      var task = GetOrBuildFiguresOnRoadAsync("MKAD", token, 10000);
      tasks.Add(task);

      var task1 = GetOrBuildFiguresOnRoadAsync("CKAD", token, 10000);
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

      List<TrackPointDTO> figures = new List<TrackPointDTO>();
        //= await _testClient.GetByParams("track_name", resFolder, start_id, max_circles);

      int rnd = 0;

      for (int m = 0; m < maxMachines; m += max_circles)
      {
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

          var figure = new TrackPointDTO()
          {
            timestamp = DateTime.UtcNow,
            figure = new GeoObjectDTO()
            {
              radius = 50,
              zoom_level = i == 0 ? "" : zoom_level,
              location = start
            },
            extra_props = extra_props
          };
          figures.Add(figure);
        }
        await _testClient.AddTracks(figures);
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

      var geometry_road = mkadPolyline.geometry as GeometryPolylineDTO;

      var sToSave = JsonSerializer.Serialize(
        geometry_road,
        new JsonSerializerOptions()
        {
          Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
          WriteIndented = true,
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull
        }
      );

      try
      {
        File.WriteAllText($"D:\\TESTS\\Leaflet\\leaflet_data\\{resFolder}.json", sToSave.ToString());
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }

      while (!token.IsCancellationRequested)
      {
        await BuildMachines(resFolder, geometry_road, maxMachines, zoom_level);
        await Task.Delay(1000);
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
