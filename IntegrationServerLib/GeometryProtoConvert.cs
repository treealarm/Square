using Common;
using Domain;

namespace IntegrationServerLib
{
  /// <summary>
  /// Конвертация геометрии между proto (Square) и DTO. Вынесено из GRPCServiceProxy,
  /// чтобы серверный приёмный слой и northbound (ProtoToDTOConvertor/IntegroController)
  /// зависели от общего хелпера, а не друг от друга.
  /// </summary>
  public static class GeometryProtoConvert
  {
    public static GeometryDTO CoordsFromProto2DTO(ProtoGeometry geometry)
    {
      GeometryDTO geo;

      if (geometry.Type == "Polygon" || geometry.Type == "LineString")
      {
        var polygonCoord = new GeometryPolygonDTO();
        geo = polygonCoord;
        polygonCoord.coord = new List<Geo2DCoordDTO>();

        foreach (var c in geometry.Coord)
        {
          polygonCoord.coord.Add(new Geo2DCoordDTO()
          {
            Lon = c.Lon,
            Lat = c.Lat
          });
        }
      }
      else
      if (geometry.Type == "Point")
      {
        var c = geometry.Coord.FirstOrDefault();

        if (c == null)
        {
          return null;
        }
        var pointCoord = new GeometryCircleDTO();
        geo = pointCoord;
        pointCoord.coord = new Geo2DCoordDTO()
        {
          Lon = c.Lon,
          Lat = c.Lat
        };
      }
      else
      {
        return null;
      }
      return geo;
    }

    public static ProtoGeometry ConvertGeoDTO2Proto(GeometryDTO location)
    {
      ProtoGeometry protoGeometry = new ProtoGeometry();

      if (location is GeometryCircleDTO point)
      {
        protoGeometry.Type = "Point";  // Устанавливаем тип
        protoGeometry.Coord.Add(
            new ProtoCoord { Lat = point.coord.Lat, Lon = point.coord.Lon });
      }

      if (location is GeometryPolygonDTO polygon)
      {
        protoGeometry.Type = "Polygon";  // Устанавливаем тип

        foreach (var coord in polygon.coord)
        {
          protoGeometry.Coord.Add(new ProtoCoord { Lat = coord.Lat, Lon = coord.Lon });
        }

        // Закрываем полигон, добавив первую координату в конец списка
        protoGeometry.Coord.Add(new ProtoCoord { Lat = polygon.coord[0].Lat, Lon = polygon.coord[0].Lon });
      }

      if (location is GeometryPolylineDTO line)
      {
        protoGeometry.Type = "LineString";  // Устанавливаем тип

        foreach (var coord in line.coord)
        {
          protoGeometry.Coord.Add(new ProtoCoord { Lat = coord.Lat, Lon = coord.Lon });
        }
      }

      return protoGeometry;
    }
  }
}
