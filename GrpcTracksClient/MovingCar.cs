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
  private readonly long _number;

  private List<ProtoCoord> _route = new();
  private int _currentRouteIndex = 0;
  private ProtoCoord _currentPosition;
  private double _currentDistance = 0;
  private double _segmentDistance;
  private double _azimuth;
  private ProtoFig _figure;
  private readonly string _id;
  private double _speed_ms = 0;
  public string Id { get { return _id; } }

  public MovingCar(ValhallaRouter router, long number)
  {
    _router = router;
    _number = number;
    _id = Utils.LongTo24String(_number);
    _color = GenerateRandomColor();
    _carIndex = _random.Next(0, _carImages.Length);
    _currentPosition = GenerateRandomStartPosition();
    _figure = CreateFigure();
  }

  public E_CarStates CarState { get; set; } = E_CarStates.Free;
  public int Counter { get; set; } = 0;
  private ProtoFig CreateFigure()
  {
    var fig = new ProtoFig
    {
      Id = _id,
      Name = "Car " + _number,
      Geometry = new ProtoGeometry { Type = "Point" }
    };
    fig.Radius = 50;

    fig.ExtraProps.Add(new ProtoObjExtraProperty { PropName = "track_name", StrVal = "lisa_alert" });
    fig.ExtraProps.Add(new ProtoObjExtraProperty { PropName = "__color", StrVal = _color, VisualType = "__clr" });
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
  private async Task GenerateNewRouteAsync()
  {
    var endLat = GetRandomDouble(55.750, 55.770);
    var endLon = GetRandomDouble(37.567, 37.680);

    _route = await _router.GetRoute(_currentPosition, new ProtoCoord { Lat = endLat, Lon = endLon });
    _currentRouteIndex = 0;
    PrepareNextSegment();
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
