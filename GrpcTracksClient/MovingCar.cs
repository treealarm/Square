using Common;
using Domain;
using GrpcTracksClient;
using LeafletAlarmsGrpc;
using ValhallaLib;

public class MovingCar
{
  private static readonly Random _random = new Random();
  private readonly ValhallaRouter _router;
  private readonly string[] _carImages = {
        @"images/car_red_256.png",
        @"images/car_taxi.png",
        @"images/car_police.png"
    };
  private readonly string _color;
  private readonly int _carIndex;
  private readonly string _name;

  private List<ProtoCoord> _route = new();
  private int _currentRouteIndex = 0;
  private ProtoCoord _currentPosition;
  private double _currentDistance = 0;
  private double _segmentDistance;
  private double _azimuth;
  private ProtoFig _figure;
  private readonly string _id;
  private readonly string _parent_id;
  private double _speed_ms = 0;
  public string Id { get { return _id; } }
  public string ParentId { get { return _parent_id; } }

  public MovingCar(ValhallaRouter router, string name, string id, string parent_id)
  {
    _router = router;
    _name = name;
    _id = id;
    _parent_id = parent_id;
    _color = GenerateRandomColor();
    _carIndex = _random.Next(0, _carImages.Length);
    _currentPosition = GenerateRandomStartPosition();
    _figure = CreateFigure();
  }

  private E_CarStates _carState = E_CarStates.Free;
  public E_CarStates CarState
  {
    get => _carState;
    set
    {
      _carState = value;
      if (_carState == E_CarStates.Free)
      {
        Task.Run(() => GenerateNewRouteAsync()); // Генерируем случайный маршрут
      }
    }
  }
  public int Counter { get; set; } = 0;
  public string StringParam { get; set; } = string.Empty;
  public ProtoCoord DestinationPos
  {
    get => _currentPosition;
    set
    {
      if (CarState != E_CarStates.Occupated)
      {
        CarState = E_CarStates.Occupated;
      }
      Task.Run(() => GenerateNewRouteAsync(value)); // Строим маршрут до заданной точки
    }
  }

  private ProtoFig CreateFigure()
  {
    var fig = new ProtoFig
    {
      Id = _id,
      Name = _name,
      Geometry = new ProtoGeometry { Type = "Point" },
      ParentId = ParentId
    };
    fig.Radius = 50;

    fig.ExtraProps.Add(new ProtoObjExtraProperty { PropName = "track_name", StrVal = "lisa_alert" });
    fig.ExtraProps.Add(new ProtoObjExtraProperty { PropName = "__color", StrVal = _color, VisualType = VisualTypes.Color });
    fig.ExtraProps.Add(new ProtoObjExtraProperty { PropName = "__image", StrVal = _carImages[_carIndex] });
    fig.ExtraProps.Add(new ProtoObjExtraProperty { PropName = "__image_rotate", StrVal = ((int)_azimuth).ToString() });
    fig.Geometry.Coord.Add(new ProtoCoord { Lat = _currentPosition.Lat, Lon = _currentPosition.Lon });
    return fig;
  }

  public async Task<ProtoFig> DoOneStep()
  {

    // Если текущий маршрут завершён, создаём новый
    if (_currentRouteIndex >= _route.Count - 1 && _currentDistance >= _segmentDistance)
    {
      if (CarState == E_CarStates.Occupated)
      {
        // Машина достигла конечной точки в режиме Occupated
        return CreateFigure();
      }
      await GenerateNewRouteAsync();
    }

    // Если текущий сегмент завершён, переходим к следующему
    if (_currentDistance >= _segmentDistance)
    {
      _currentRouteIndex++;
      if (_currentRouteIndex < _route.Count - 1)
      {
        PrepareNextSegment();
      }
    }

    // Выполняем шаг
    _speed_ms = GetRandomDouble(7, 12); // Speed in m/s so it is meeters per second

    _currentDistance += _speed_ms;
    if (_currentDistance > _segmentDistance)
    {
      _currentDistance = _segmentDistance + 0.1; // Не выходить за пределы сегмента
    }

    var curCoords = GeoCalculator.CalculateCoordinates(
        _currentPosition.Lat,
        _currentPosition.Lon,
        _speed_ms,
        _azimuth
    );

    // Обновляем текущую позицию
    _currentPosition = new ProtoCoord { Lat = curCoords.latitude, Lon = curCoords.longitude };

    return CreateFigure();
  }

  public ValuesProto GetValuesToSend()
  {
    var valuesToSend = new ValuesProto();
    var randomValue = _random.Next(50, 80);

    valuesToSend.Values.Add(new ValueProto()
    {
      OwnerId = _id,
      Name = "speed",
      Value = new ValueProtoType() { IntValue = (int)(_speed_ms * 3.6) },
    });

    randomValue = _random.Next(80, 110);

    valuesToSend.Values.Add(new ValueProto()
    {
      OwnerId = _id,
      Name = "temperature",
      Value = new ValueProtoType() { IntValue = randomValue },
    });
    return valuesToSend;
  }
  private async Task GenerateNewRouteAsync(ProtoCoord targetPoint = null)
  {
    if (targetPoint!=null)
    {
      // Если задана конечная точка, строим маршрут к ней
      _route = await _router.GetRoute(_currentPosition, targetPoint);
    }
    else
    {
      // Случайная конечная точка для свободного состояния
      var endLat = GetRandomDouble(55.750, 55.770);
      var endLon = GetRandomDouble(37.567, 37.680);
      _route = await _router.GetRoute(_currentPosition, new ProtoCoord { Lat = endLat, Lon = endLon });
    }

    _currentRouteIndex = 0;
    if (_route.Count > 1)
    {
      PrepareNextSegment();
    }
  }

  private void PrepareNextSegment()
  {
    var start = _route[_currentRouteIndex];
    var end = _route[_currentRouteIndex + 1];
    _segmentDistance = GeoCalculator.CalculateDistance(start.Lat, start.Lon, end.Lat, end.Lon);
    _azimuth = GeoCalculator.CalculateAzimuth(start.Lat, start.Lon, end.Lat, end.Lon);
    _currentDistance = 0; // Сбрасываем пройденное расстояние для нового сегмента
    _currentPosition = start; // Начальная точка нового сегмента
  }

  private ProtoCoord GenerateRandomStartPosition()
  {
    return new ProtoCoord
    {
      Lat = GetRandomDouble(55.750, 55.770),
      Lon = GetRandomDouble(37.567, 37.680)
    };
  }

  private static string GenerateRandomColor()
  {
    return $"#{_random.Next(20):X2}{_random.Next(256):X2}{_random.Next(100):X2}";
  }

  private static double GetRandomDouble(double min, double max)
  {
    return _random.NextDouble() * (max - min) + min;
  }
}
