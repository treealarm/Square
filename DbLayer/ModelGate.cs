using Domain;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        radius = dbObj.radius,
        zoom_level = dbObj.zoom_level,
        location = ConvertGeoDB2DTO(dbObj.figure)
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


  public static List<T> ConvertExtraPropsToDB<T>(List<ObjExtraPropertyDTO> extra_props)
    where T : DBObjExtraProperty, new()
    {
      if (extra_props == null)
        return null;

      var ep_db = new List<T>();

      // Исключаем свойства, которые есть в других DTO
      var propertyNames = typeof(FigureZoomedDTO).GetProperties().Select(x => x.Name).ToList();
      propertyNames.AddRange(typeof(FigureGeoDTO).GetProperties().Select(x => x.Name));

      foreach (var prop in extra_props)
      {
        if (propertyNames.Contains(prop.prop_name))
          continue;

        var newProp = new T
        {
          prop_name = prop.prop_name,
          visual_type = prop.visual_type
        };

        if (prop.visual_type == VisualTypes.Double)
        {
          if (double.TryParse(prop.str_val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            newProp.str_val = result.ToString();
        }
        else if (prop.visual_type == VisualTypes.DateTime)
        {
          if (DateTime.TryParse(prop.str_val, out var dt))
            newProp.str_val = dt.ToUniversalTime().ToString();
        }

        if (newProp.str_val == null)
          newProp.str_val = prop.str_val;

        ep_db.Add(newProp);
      }

      return ep_db;
    }


    public static string GetDefaultVisualType(DBObjExtraProperty prop)
    {
      if(!string.IsNullOrEmpty(prop.visual_type))
      {
        return prop.visual_type;
      }

      if (prop.prop_name == "__color")
      {
        return VisualTypes.Color;
      }
      return prop.visual_type;
    }
    // Конвертация списка дочерних свойств в DTO
    public static List<ObjExtraPropertyDTO> ConverDBExtraProp2DTO<T>(List<T> props)
        where T : DBObjExtraProperty
    {
      var retVal = new List<ObjExtraPropertyDTO>();

      if (props == null)
        return retVal;

      foreach (var prop in props)
      {
        retVal.Add(new ObjExtraPropertyDTO
        {
          prop_name = prop.prop_name,
          str_val = prop.str_val.ToString(),
          visual_type = GetDefaultVisualType(prop)
        });
      }

      return retVal;
    }

    // Конвертация родителя (DBMarkerProperties, DBEvent и др.) в DTO
    public static ObjPropsDTO Conver2Property2DTO<TParent, TChild>(TParent props)
        where TParent : class
        where TChild : DBObjExtraProperty
    {
      if (props == null)
        return null;

      // Получаем свойство extra_props через reflection, чтобы было общее для любых TParent
      var extraPropsProp = typeof(TParent).GetProperty("extra_props");
      var extraProps = extraPropsProp?.GetValue(props) as List<TChild>;

      return new ObjPropsDTO
      {
        extra_props = ConverDBExtraProp2DTO(extraProps),
        id = props.ToString() // или можно взять конкретное поле id через reflection
      };
    }

  }
}
