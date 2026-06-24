using Domain;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using NetTopologySuite;


namespace DbLayer
{
  class ModelGate
  {
    static public DBGeoObject ConvertDTO2DB(GeoObjectDTO dto)
    {
      var res = new DBGeoObject()
      {
        id = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Domain.Utils.NewGuid(),
        radius = dto.radius,
        zoom_level = dto.zoom_level,
        figure = ConvertGeoDTO2DB(dto.location)
      };

      return res;
    }

    static public List<DBGeoObject> ConvertListDTO2DB(List<GeoObjectDTO> dto)
    {
      var res = new List<DBGeoObject>();
     
      foreach (var item in dto)
      {
        res.Add(ConvertDTO2DB(item));
      }

      return res;
    }

    static public Dictionary<string, GeoObjectDTO> ConvertListDB2DTO(List<DBGeoObject> dto)
    {
      var res = new Dictionary<string, GeoObjectDTO>();

      if (dto == null)
      {
        return res;
      }
      foreach (var item in dto)
      {
        res.Add(Domain.Utils.ConvertGuidToObjectId(item.id), ConvertDB2DTO(item));
      }

      return res;
    }

    static public Geometry ConvertGeoDTO2DB(GeometryDTO location)
    {
      if (location == null)
        return null;

      var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
      Geometry ret = null;

      switch (location)
      {
        case GeometryCircleDTO point:
          ret = geometryFactory.CreatePoint(new Coordinate(point.coord.Lon, point.coord.Lat));
          break;

        case GeometryPolygonDTO polygon:
          // формируем массив координат для внешнего кольца
          var coords = new Coordinate[polygon.coord.Count + 1];
          for (int i = 0; i < polygon.coord.Count; i++)
          {
            coords[i] = new Coordinate(polygon.coord[i].Lon, polygon.coord[i].Lat);
          }
          // закрываем полигон (последняя точка = первая)
          coords[polygon.coord.Count] = new Coordinate(polygon.coord[0].Lon, polygon.coord[0].Lat);

          var linearRing = geometryFactory.CreateLinearRing(coords);
          ret = geometryFactory.CreatePolygon(linearRing);
          break;

        case GeometryPolylineDTO line:
          var lineCoords = new Coordinate[line.coord.Count];
          for (int i = 0; i < line.coord.Count; i++)
          {
            lineCoords[i] = new Coordinate(line.coord[i].Lon, line.coord[i].Lat);
          }
          ret = geometryFactory.CreateLineString(lineCoords);
          break;

        default:
          throw new NotSupportedException($"GeometryDTO type {location.GetType().Name} not supported");
      }

      return ret;
    }

    static public GeoObjectDTO ConvertDB2DTO(DBGeoObject dbObj)
    {
      if (dbObj == null)
      {
        return null;
      }

      GeoObjectDTO retVal = new GeoObjectDTO()
      {
        id = Domain.Utils.ConvertGuidToObjectId(dbObj.id),
        Version = dbObj.Version,
        radius = dbObj.radius,
        zoom_level = dbObj.zoom_level,
        location = ConvertGeoDB2DTO(dbObj.figure),
      };

      return retVal;
    }

  static private GeometryDTO ConvertGeoDB2DTO(Geometry location)
  {
    if (location == null)
      return null;

    GeometryDTO ret = null;

    switch (location)
    {
      case Point point:
        ret = new GeometryCircleDTO(
            new Geo2DCoordDTO { Y = point.Y, X = point.X }
        );
        break;

      case Polygon polygon:
        var retPolygon = new GeometryPolygonDTO();
        // Берем внешнее кольцо полигона
        var coords = polygon.ExteriorRing.Coordinates;
        foreach (var cur in coords)
        {
          retPolygon.coord.Add(new Geo2DCoordDTO { Y = cur.Y, X = cur.X });
        }
        // Убираем последнюю точку, если совпадает с первой (как в GeoJSON)
        if (retPolygon.coord.Count > 3 &&
            retPolygon.coord[0].X == retPolygon.coord[^1].X &&
            retPolygon.coord[0].Y == retPolygon.coord[^1].Y)
        {
          retPolygon.coord.RemoveAt(retPolygon.coord.Count - 1);
        }
        ret = retPolygon;
        break;

      case LineString line:
        var retLine = new GeometryPolylineDTO();
        foreach (var cur in line.Coordinates)
        {
          retLine.coord.Add(new Geo2DCoordDTO {Y = cur.Y, X = cur.X });
        }
        ret = retLine;
        break;

      default:
        throw new NotSupportedException(
            $"Geometry type {location.GeometryType} not supported");
    }

    ret.type = location.GeometryType; // например "Point", "Polygon", "LineString"
    return ret;
  }

  }
}
