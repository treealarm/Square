using Common;
using LeafletAlarmsGrpc;
using ObjectActions;
using System.Collections.Concurrent;
using ValhallaLib;

namespace GrpcTracksClient.Services
{
  public class MoveObjectService : IMoveObjectService, IObjectActions
  {
    private static ValhallaRouter _router = new ValhallaRouter();

    private ConcurrentDictionary<string, MovingCar> _cars = 
      new ConcurrentDictionary<string, MovingCar>();
    private ProtoObject _mainObject = null;
    private bool _inited = false;
    private const string _main_str = "main";
    private const string _car_str = "car";
    public MoveObjectService()
    {
      
    }

    public async Task<ProtoObject> GetBaseObject(string id_in)
    {
      var client = Utils.Client;

      var ids = new ProtoObjectIds();
      ids.Ids.Add(id_in);
      var response = await client.RequestObjects(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.FirstOrDefault();
    }
    public async Task<List<ProtoObject>> GetBaseObjects(IEnumerable<string> ids_in)
    {
      var client = Utils.Client;

      var ids = new ProtoObjectIds();
      ids.Ids.AddRange(ids_in);
      var response = await client.RequestObjects(ids);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }

    public async Task<List<IntegroProto>> GetIntegroObjects(string type)
    {
      var client = Utils.Client;

      var integroRequest = new GetListByTypeRequest();
      integroRequest.IName = client.AppId;
      integroRequest.IType = type;

      // Ищем уже созданный main объект по типу
      var response = await client.GetListByType(integroRequest);

      if (response == null)
      {
        return null;
      }
      return response.Objects.ToList();
    }

    public async Task<ProtoObject> UpdateBaseObject(string id, string name)
    {
      var client = Utils.Client;

      var mainObject = new ProtoObject()
      {
        Id = id,
        Name = name
      };
      var list = new ProtoObjectList();
      list.Objects.Add(mainObject);
      var response = await client.UpdateObjects(list);

      if (response == null)
      {
        return null;
      }
      return response.Objects.FirstOrDefault();
    }
    public async Task InitMainObject(CancellationToken token)
    {
      IntegroProto mainIntegro = null;

      while (!_inited && !token.IsCancellationRequested)
      {
        await Task.Delay(500);
        var client = Utils.Client;

        // Ищем уже созданный main объект по типу
        var integroObjects = await GetIntegroObjects(_main_str);

        if (integroObjects != null)
        {
          if (integroObjects.Count > 0)
          {
            //Если находим, то обращаемся к таблице Objects
            // И получаем базовый объект с именем, айди, итд
            mainIntegro = integroObjects.FirstOrDefault();

            if (mainIntegro != null)
            {
              //Request real object or create if doesn't exist
              var mainObj = await GetBaseObject(mainIntegro.ObjectId);

              if (mainObj == null)
              {
                // Если не находим, то создаем с дефолтным именем
                mainObj = await UpdateBaseObject(mainIntegro.ObjectId, $"{client.AppId}_{_main_str}");                
              }

              if (mainObj != null)
              {
                _mainObject = mainObj;
                _inited = true;
              }
            }            
          }
          else
          {
            //Если не нашли объект в БД, то создадим новый.
            var mainUid = await Utils.GenerateObjectId(_main_str, 0);

            if (!string.IsNullOrEmpty(mainIntegro?.ObjectId))
            {
              mainUid = mainIntegro.ObjectId;
            }

            var integro = new IntegroListProto();
            integro.Objects.Add(new IntegroProto()
            {
              IType = _main_str,
              IName = client.AppId,
              ObjectId = mainUid
            });
            await client.UpdateIntegro(integro);
          }
        }
      }
    }

    public async Task InitCarObjects(CancellationToken token)
    {
      while (true && !token.IsCancellationRequested)
      {
        await Task.Delay(500);
        var client = Utils.Client;
        var integroObjects = await GetIntegroObjects(_car_str);

        if (integroObjects == null)
        {
          continue;
        }

        int start_car_index = IMoveObjectService.MaxCars;
        if (integroObjects != null)
        {
          start_car_index = integroObjects.Count;
        }

        var integroRequest = new IntegroListProto();

        for (long carId = start_car_index; carId < IMoveObjectService.MaxCars; carId++)
        {
          var carNum = carId + 1;
          var carUid = await Utils.GenerateObjectId(_car_str, carNum);

          integroRequest.Objects.Add(new IntegroProto()
          {
            IName = client.AppId,
            ObjectId = carUid,
            IType = _car_str
          });
        }
        await client.UpdateIntegro(integroRequest);
        integroObjects = await GetIntegroObjects(_car_str);
        if (integroObjects == null || integroObjects.Count < IMoveObjectService.MaxCars)
        {
          continue;
        }

        var baseObjects = await GetBaseObjects(integroObjects.Select(o => o.ObjectId));
        if (baseObjects == null)
        {
          continue;
        }

        foreach (var baseObj in baseObjects)
        {
          var movingCar = new MovingCar(_router, baseObj.Name, baseObj.Id);
          _cars.TryAdd(movingCar.Id, movingCar);
        }

        start_car_index = baseObjects.Count;
        for (long carId = start_car_index; carId < IMoveObjectService.MaxCars; carId++)
        {
          try
          {
            var carNum = carId + 1;
            var carUid = await Utils.GenerateObjectId(_car_str, carNum);
            var movingCar = new MovingCar(_router, $"Car {carNum}", carUid);
            _cars.TryAdd(movingCar.Id, movingCar);
          }
          catch (Exception ex)
          {
            Logger.LogException(ex);
          }
        }

        if (_cars.Count >= IMoveObjectService.MaxCars)
        {
          break;
        }
      }
    }

    public async Task MoveCars(CancellationToken token)
    {
      await InitMainObject(token);
      await InitCarObjects(token);

      var client = Utils.Client;
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
            var integroRequest = new IntegroListProto();
            foreach (var fig in figs.Figs)
            {
              integroRequest.Objects.Add(new IntegroProto()
              {
                IName = client.AppId,
                ObjectId = fig.Id,
                IType = _car_str
              });
              Console.WriteLine($"Register integro:{client.AppId}:{fig.Id}");
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

      var client = Utils.Client;

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

      var client = Utils.Client;
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
    private static string CarCreateActionName = "CreateCar";
    private void FillCarAction(ProtoGetAvailableActionsRequest request, ProtoGetAvailableActionsResponse response)
    {
      if (_cars.TryGetValue(request.ObjectId, out var car))
      {
        var action = new ProtoActionDescription
        {
          Name = car.CarState.HasFlag(E_CarStates.Occupated) ? "Free" : "Occupate"
        };

        response.ActionsDescr.Add(action);
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
          Name = nameof(car.DestinationPos),
          CurVal = new ProtoActionValue()
          {
            Coordinates = new ProtoGeometry()
            {
              Type = "Point"
            }
          }
        };
        coordParam.CurVal.Coordinates.Coord.Add(car.DestinationPos);
        action1.Parameters.Add(coordParam);
        response.ActionsDescr.Add(action1);
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
        response.ActionsDescr.Add(action2);
      }
    }

    private void FillCreateAction(ProtoGetAvailableActionsRequest request, ProtoGetAvailableActionsResponse response)
    {
      var action1 = new ProtoActionDescription
      {
        Name = CarCreateActionName
      };

      action1.Parameters.Add(new ProtoActionParameter()
      {
        Name = "Name",
        CurVal = new ProtoActionValue()
        {
          StringValue = "New Car"
        }
      });

      if (_cars.TryGetValue(request.ObjectId, out var car))
      {
        action1.Parameters.Add(new ProtoActionParameter()
        {
          Name = "ParentId",
          CurVal = new ProtoActionValue()
          {
            StringValue = request.ObjectId
          }
        });
      }
      response.ActionsDescr.Add(action1);
    }
    public async Task<ProtoGetAvailableActionsResponse> GetAvailableActions(ProtoGetAvailableActionsRequest request)
    {
      await Task.Delay(0);
      ProtoGetAvailableActionsResponse response = new ProtoGetAvailableActionsResponse();

      FillCarAction(request, response);
      FillCreateAction(request, response);
      return response;
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

            var coordVal = action.Parameters.Where(i => i.Name == nameof(car.DestinationPos)).FirstOrDefault()?.CurVal.Coordinates ?? null;
            if (coordVal != null)
            {
              car.DestinationPos = coordVal.Coord.FirstOrDefault();
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
