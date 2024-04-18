using GrpcDaprLib;
using LeafletAlarmsGrpc;
using ValhallaLib;

namespace GrpcTracksClient
{
  internal class MoveObject
  {
    private static ValhallaRouter _router = new ValhallaRouter();

    public static async Task Move()
    {     
      //var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      //var s = await ResourceLoader.GetResource(resourceName);

      //var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);
      var figSenderTask = CarFigureSender();

      List<Task> listCarsTasks = new List<Task>();

      for (long carId = 1; carId < 50; carId++)
      {
        try
        {
          var taskCar = MoveGrpcCar(carId);
          listCarsTasks.Add(taskCar);
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
        }
      }

      try
      {
        await MovePolygon();
      }
      catch(Exception ex)
      {
        Logger.LogException(ex);

        if (ex.InnerException != null)
        {
          Logger.LogException(ex.InnerException);
        }
      }

      Task.WaitAll(listCarsTasks.ToArray());
      Task.WaitAll(figSenderTask);
    }


    private static async Task MovePolygon()
    {

      var grpcTask = MoveGrpcPolygon();

      try
      {
        await Move2();
      }
      catch (Exception ex)
      {
        Logger.LogException(ex);
      }

      Task.WaitAll(grpcTask);
    }

    public static async Task Move2()
    {
      var figs = new ProtoFigures();
      var fig = new ProtoFig();
      figs.Figs.Add(fig);

      fig.Id = "6423e54d513bfe83e9d59794";
      fig.Name = "Triangle";
      fig.Geometry = new ProtoGeometry();
      fig.Geometry.Type = "Polygon";
      figs.AddTracks = true;



      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert"
      });


      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert3"
      });

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.7566737398449,
        Lon = 37.60722931951715
      });

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.748852242908995,
        Lon = 37.60259563134112
      });

      fig.Geometry.Coord.Add(new ProtoCoord()
      {
        Lat = 55.75203896803514,
        Lon = 37.618727730916895
      });

      using var client = new GrpcUpdater();

      double step = 0.001;

      for (int i = 0; i < 100; i++)
      {
        try
        {
          if (i > 50)
          {
            step = -0.001;
          }
          foreach (var f in fig.Geometry.Coord)
          {
            f.Lat += step;
            f.Lon += step;
          }
          try
          {
            var reply =
            await client.Move(figs);
            //Console.WriteLine("Fig DAPR: " + reply?.ToString());
          }
          catch (Exception ex)
          {
            Logger.LogException(ex);
            await Task.Delay(10000);
          }

        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
          break;
        }
        await Task.Delay(1000);
      }
    }

    public static async Task MoveGrpcPolygon()
    {
      var figs = new ProtoFigures();
      var fig = new ProtoFig();
      figs.Figs.Add(fig);

      fig.Id = "6423e54d513bfe83e9d59793";
      fig.Name = "Test";
      fig.Geometry = new ProtoGeometry();
      fig.Geometry.Type = "Polygon";
      figs.AddTracks = true;



      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert"
      });

      fig.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "cloud"
      });

      Random random = new Random();

      var center = new ProtoCoord()
      {
        Lat = 55.753201,
        Lon = 37.621130
      };

      using var client = new GrpcUpdater();
      client.Connect(null);
      var step = 0.0001;

      for (int i = 0; i < 1000000; i++)
      {
        var propColor = fig.ExtraProps.Where(p => p.PropName == "__color").FirstOrDefault();

        if (propColor == null)
        {
          propColor = new ProtoObjExtraProperty()
          {
            PropName = "__color",
            StrVal = string.Empty
          };
          fig.ExtraProps.Add(propColor);
        }
        propColor.StrVal = $"#{_random.Next(100, 150).ToString("X2")}{_random.Next(100, 150).ToString("X2")}{_random.Next(100, 256).ToString("X2")}";

        center.Lat += random.Next(-5, +5) * step;
        center.Lon += random.Next(-5, +5) * step;

        fig.Geometry.Coord.Clear();
        var rad = 400;

        var aStep = random.Next(20, 90);

        for (double a = 0; a < 360; a += aStep)
        {
          rad += random.Next(-20, +20);
          var pt = GeoCalculator.CalculateCoordinates(center.Lat, center.Lon, rad, a);
          fig.Geometry.Coord.Add(new ProtoCoord()
          {
            Lat = pt.latitude,
            Lon = pt.longitude
          });
        }

        var newFigs = await client.Move(figs);
        //Console.WriteLine("Fig GRPC: " + newFigs?.ToString());
        await Task.Delay(1000);
      }
    }



    static string LongTo24String(long number)
    {
      return "1111" + number.ToString("D20");
    }
    private static Random _random = new Random();

    static double GetRandomDouble(double min, double max)
    {
      return min + (_random.NextDouble() * (max - min));
    }
    public static async Task MoveGrpcCar(long number)
    { 
      var endLat = GetRandomDouble(55.750, 55.770);
      var endLon = GetRandomDouble(37.567, 37.680);

      var color =
        $"#{_random.Next(20).ToString("X2")}{_random.Next(256).ToString("X2")}{_random.Next(100).ToString("X2")}";
      int carNum = _random.Next(0, 3);
      var carArr = new string[] {
        @"images/car_red_256.png",
        @"images/car_taxi.png",
        @"images/car_police.png"};
      var fig_Id = LongTo24String(number); //"6423e54d513bfe83e9d59794";
      var fig_Name = "TestCar" + number.ToString();

      for (int h = 0; h < 10; h++)
      {
        var startLat = endLat;
        var startLon = endLon;

        var prev = new ProtoCoord() { Lat = startLat, Lon = startLon };

        endLat = GetRandomDouble(55.750, 55.770);
        endLon = GetRandomDouble(37.567, 37.680);

        //{"lat":55.76034990679016,"lon":37.56721972720697},{"lat":55.75919085473824,"lon":37.679829590488225}
        var testReturn = await _router.GetRoute(
          new ProtoCoord() { Lat = startLat, Lon = startLon },
          new ProtoCoord() { Lat = endLat, Lon = endLon }
        );

        foreach (var track in testReturn)
        {
          var fig = new ProtoFig();

          fig.Id = fig_Id;
          fig.Name = fig_Name;

          fig.Geometry = new ProtoGeometry();
          fig.Geometry.Type = "Point";
          fig.Radius = 50;

          fig.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = "track_name",
            StrVal = "lisa_alert"
          });

          fig.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = "__color",
            StrVal = color
          });

          
          fig.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = @"__image",
            StrVal = carArr[carNum]
          });

          var rotate = new ProtoObjExtraProperty()
          {
            PropName = @"__image_rotate",
            StrVal = @"0"
          };

          fig.ExtraProps.Add(rotate);

          

          var azimuth = GeoCalculator.CalculateAzimuth(prev.Lat, prev.Lon, track.Lat, track.Lon);
          var distance = GeoCalculator.CalculateDistance(prev.Lat, prev.Lon, track.Lat, track.Lon);

          int rot = (int)azimuth;
          rotate.StrVal = rot.ToString();
          
          var cur_speed = GetRandomDouble(7, 12);

          for (double cur_distance = 0;  cur_distance <= distance; cur_distance += cur_speed) //cur_speed m/s
          {
            fig.Geometry.Coord.Clear();

            var cur_coords = GeoCalculator.CalculateCoordinates(prev.Lat, prev.Lon, cur_distance, azimuth);

            fig.Geometry.Coord.Add(new ProtoCoord()
            {
              Lat = cur_coords.latitude,
              Lon = cur_coords.longitude //x
            });

            if (AddFigToSend(fig) > 1000)
            {
              await Task.Delay(1500);
            }
            await Task.Delay(500);
          }
          
          prev = track;

          
          await Task.Delay(500);
        }
      }
    }

    static List<ProtoFig> _figsToSend = new List<ProtoFig>();
    private static int AddFigToSend(ProtoFig f)
    {
      lock (_figsToSend)
      {
        _figsToSend.Add(f);
        return _figsToSend.Count;
      }
    }

    private static bool _working = true;
    private static async Task CarFigureSender()
    {
      using var client = new GrpcUpdater();
      client.Connect(null);

      while (_working)
      {
        ProtoFigures figs = new ProtoFigures();

        lock (_figsToSend)
        {
          foreach (var f in _figsToSend)
          {
            figs.Figs.Add(f);
          }
          _figsToSend = new List<ProtoFig>();
        }

        if (figs.Figs.Any())
        { 
          try
          {
            figs.AddTracks = true;
            var newFigs = await client.Move(figs);
          }
          catch (Exception ex)
          {
            Logger.LogException(ex);
          }
        }
        await Task.Delay(1000);
      }
    }
  }
}
