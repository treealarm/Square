using GrpcDaprLib;
using LeafletAlarmsGrpc;
using System.Diagnostics.Metrics;
using ValhallaLib;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GrpcTracksClient
{
  internal class MoveObject
  {
    private static ValhallaRouter _router = new ValhallaRouter();
    public static int MaxCars = 50;
    public static async Task MoveCars(CancellationToken token)
    {
      List<MovingCar> listCars = new List<MovingCar>();

      for (long carId = 0; carId < MaxCars; carId++)
      {
        try
        {
          var movingCar = new MovingCar(_router, carId + 1);
          listCars.Add(movingCar);
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
        }
      }

      while (!token.IsCancellationRequested)
      {
        try
        {
          // Запускаем DoOneStep для всех машин и собираем результаты
          var tasks = listCars.Select(car => car.DoOneStep()).ToList();
          var results = await Task.WhenAll(tasks);

          // Обрабатываем результаты
          foreach (var result in results)
          {
            if (result != null) // Проверяем результат, если нужно
            {
              // Здесь можно добавить логику обработки результата
              AddFigToSend(result);
            }
          }
        }
        catch (Exception ex)
        {
          Logger.LogException(ex); // Логируем исключения
        }

        await Task.Delay(1000); // Задержка между итерациями и скорость в метрах в секунду
      }
    }


    public static async Task MovePolygons()
    {     
      //var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      //var s = await ResourceLoader.GetResource(resourceName);

      //var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);
      var figSenderTask = CarFigureSender();

      

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
      var step = 0.0001;

      for (int i = 0; i < 1000000; i++)
      {
        var propColor = fig.ExtraProps.Where(p => p.PropName == "__color").FirstOrDefault();

        if (propColor == null)
        {
          propColor = new ProtoObjExtraProperty()
          {
            PropName = "__color",
            StrVal = string.Empty,
            VisualType = "__clr"
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

    private static Random _random = new Random();

    static double GetRandomDouble(double min, double max)
    {
      return min + (_random.NextDouble() * (max - min));
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
      Random random = new Random();
      
      while (_working)
      {
        var valuesToSend = new ValuesProto();

        var figs = new ProtoFigures();

        lock (_figsToSend)
        {
          foreach (var f in _figsToSend)
          {
            figs.Figs.Add(f);

            var randomValue = random.Next(50, 80);

            valuesToSend.Values.Add(new ValueProto()
            {
              OwnerId = f.Id,
              Name = "speed",
              Value = new ValueProtoType() { IntValue = randomValue},
            });

            randomValue = random.Next(80, 110);

            valuesToSend.Values.Add(new ValueProto()
            {
              OwnerId = f.Id,
              Name = "temperature",
              Value = new ValueProtoType() { IntValue = randomValue },
            });
          }
          _figsToSend = new List<ProtoFig>();
        }

       

        if (figs.Figs.Any())
        { 
          try
          {
            figs.AddTracks = true;
            var newFigs = await client.Move(figs);
            var vals = await client.UpdateValues(valuesToSend);
          }
          catch (Exception ex)
          {
            Logger.LogException(ex);
          }
        }
        await Task.Delay(500);
      }
    }
  }
}
