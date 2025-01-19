using Common;
using GrpcDaprLib;
using LeafletAlarmsGrpc;
using ObjectActions;
using ValhallaLib;

namespace GrpcTracksClient.Services
{
  public class MoveObjectService : IMoveObjectService, IObjectActions
  {
    private static ValhallaRouter _router = new ValhallaRouter();

    private Dictionary<string, MovingCar> _cars = new Dictionary<string, MovingCar>();
    public MoveObjectService()
    {
      for (long carId = 0; carId < IMoveObjectService.MaxCars; carId++)
      {
        try
        {
          var movingCar = new MovingCar(_router, carId + 1);
          _cars.Add(movingCar.Id, movingCar);
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
        }
      }
    }
    public async Task MoveCars(CancellationToken token)
    {
      using var client = new GrpcUpdater();
      bool inited = false;
      while (!token.IsCancellationRequested)
      {
        var figs = new ProtoFigures();
        figs.AddTracks = true;

        var valuesToSend = new ValuesProto();

        try
        {
          // Запускаем DoOneStep для всех машин и собираем результаты
          var tasks = _cars.Values.Select(car => car.DoOneStep()).ToList();
          var results = await Task.WhenAll(tasks);

          var values = _cars.Values.Select(car => car.GetValuesToSend()).ToList();

          foreach (var vals in values)
          {
            foreach (var val in vals.Values)
            {
              valuesToSend.Values.Add(val);
            }
          }

          // Обрабатываем результаты
          foreach (var result in results)
          {
            if (result != null) // Проверяем результат, если нужно
            {
              figs.Figs.Add(result);
            }
          }
        }
        catch (Exception ex)
        {
          Logger.LogException(ex); // Логируем исключения
        }


        try
        {
          if (figs.Figs.Any())
          {
            var newFigs = await client.Move(figs);
          }
          if (valuesToSend.Values.Any())
          {
            var vals = await client.UpdateValues(valuesToSend);
          }
          if (!inited)
          {
            var integroRequest = new UpdateIntegroRequest();
            foreach (var fig in figs.Figs)
            {
              integroRequest.Objects.Add(new IntegroProto()
              {
                IName = client.AppId,
                ObjectId = fig.Id
              });
            }
            await client.UpdateIntegro(integroRequest);
            inited = true;
          }
        }
        catch (Exception ex)
        {
          Logger.LogException(ex);
        }

        await Task.Delay(1000); // Задержка между итерациями и скорость в метрах в секунду
      }
    }


    public async Task MovePolygons(CancellationToken token)
    {
      //var resourceName = $"GrpcTracksClient.JSON.SAD.json";
      //var s = await ResourceLoader.GetResource(resourceName);

      //var coords = JsonSerializer.Deserialize<GeometryPolylineDTO>(s);


      try
      {
        await MovePolygon();
      }
      catch (Exception ex)
      {
        Logger.LogException(ex);

        if (ex.InnerException != null)
        {
          Logger.LogException(ex.InnerException);
        }
      }
    }
    private async Task MovePolygon()
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

    public async Task Move2()
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

    public async Task MoveGrpcPolygon()
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
      return min + _random.NextDouble() * (max - min);
    }

    private static string CarParamsActionName = "SetCarParams";

    public async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.Delay(0);
      ProtoGetAvailableActionsResponse retVal = new ProtoGetAvailableActionsResponse();
      if (_cars.TryGetValue(request.ObjectId, out var car))
      {
        var action = new ProtoActionDescription
        {
          Name = car.CarState.HasFlag(E_CarStates.Occupated) ? "Free" : "Occupate"          
        };
       
        retVal.ActionsDescr.Add(action);
        /////Car params
        var action1 = new ProtoActionDescription
        {
          Name = CarParamsActionName
        };
        action1.Parameters.Add(new ProtoActionParameter()
        {
          Name = "Counter",
          CurVal = new ProtoActionValue()
          {
            IntValue = car.Counter
          }
        });
        action1.Parameters.Add(new ProtoActionParameter()
        {
          Name = nameof(car.StringParam),
          CurVal = new ProtoActionValue()
          {
            StringValue = car.StringParam
          }
        });

        var coordParam = new ProtoActionParameter()
        {
          Name = nameof(car.CurrentPos),
          CurVal = new ProtoActionValue()
          {
            Coordinates = new ProtoGeometry()
            {
              Type = "Point"
            }
          }
        };
        coordParam.CurVal.Coordinates.Coord.Add(car.CurrentPos);
        action1.Parameters.Add(coordParam);
        retVal.ActionsDescr.Add(action1);
        ///End  car params

        var action2 = new ProtoActionDescription
        {
          Name = "SetString"
        };
        action2.Parameters.Add(new ProtoActionParameter()
        {
          Name = nameof(car.StringParam),
          CurVal = new ProtoActionValue()
          {
            StringValue = car.StringParam
          }
        });
        retVal.ActionsDescr.Add(action2);
      }
      return retVal;
    }

    public async Task<ProtoExecuteActionResponse> ExecuteActions(ProtoExecuteActionRequest request)
    {
      ProtoExecuteActionResponse retVal = new ProtoExecuteActionResponse() { Success = true };

      await Task.Delay(0);


      foreach (var action in request.Actions)
      {
        Console.WriteLine($"ExecuteActions {action.ObjectId} {action.Name}");

        if (_cars.TryGetValue(action.ObjectId, out var car))
        {
          if (action.Name == "Free")
          {
            car.CarState = E_CarStates.Free;
          }
          if (action.Name == "Occupate")
          {
            car.CarState = E_CarStates.Occupated;
          }
          if (action.Name == CarParamsActionName)
          {
            car.Counter = action.Parameters.Where(i=>i.Name == "Counter").FirstOrDefault()?.CurVal.IntValue??0;
            var stringVal = action.Parameters.Where(i => i.Name == nameof(car.StringParam)).FirstOrDefault()?.CurVal.StringValue ?? null;
            if (stringVal != null)
            {
              car.StringParam = stringVal;
            }

            var coordVal = action.Parameters.Where(i => i.Name == nameof(car.CurrentPos)).FirstOrDefault()?.CurVal.Coordinates ?? null;
            if (coordVal != null)
            {
              car.CurrentPos = coordVal.Coord.FirstOrDefault();
            }
          }
          if (action.Name == "SetString")
          {
            car.StringParam = action.Parameters
              .Where(i => i.Name == nameof(car.StringParam))
              .FirstOrDefault()?.CurVal.StringValue ?? string.Empty;            
          }
        }
        else
        {
          retVal.Success = false;
        }
      }

      return retVal;
    }
  }
}
