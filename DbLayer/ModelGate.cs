using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  class ModelGate
  {
    static public DBGeoObject ConvertDTO2DB(GeoObjectDTO dto)
    {
      var res = new DBGeoObject()
      {
        id = dto.id,
        radius = dto.radius,
        zoom_level = dto.zoom_level,
        location = ConvertGeoDTO2DB(dto.location)
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
        res.Add(item.id, ConvertDB2DTO(item));
      }

      return res;
    }

    static public GeoJsonGeometry<GeoJson2DCoordinates> ConvertGeoDTO2DB(GeometryDTO location)
    {
      GeoJsonGeometry<GeoJson2DCoordinates> ret = null;

      if (location is GeometryCircleDTO point)
      {
        ret = new GeoJsonPoint<GeoJson2DCoordinates>(
          GeoJson.Position(point.coord.Lon, point.coord.Lat)
        );
      }

      if (location is GeometryPolygonDTO polygon)
      {
        List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

        for (int i = 0; i < polygon.coord.Count; i++)
        {
          coordinates.Add(GeoJson.Position(polygon.coord[i][1], polygon.coord[i][0]));
        }

        coordinates.Add(GeoJson.Position(polygon.coord[0][1], polygon.coord[0][0]));

        ret = GeoJson.Polygon(coordinates.ToArray());
      }

      if (location is GeometryPolylineDTO line)
      {
        List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

        for (int i = 0; i < line.coord.Count; i++)
        {
          coordinates.Add(GeoJson.Position(line.coord[i][1], line.coord[i][0]));
        }

        ret = GeoJson.LineString(coordinates.ToArray());
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
        id = dbObj.id,
        radius = dbObj.radius,
        zoom_level = dbObj.zoom_level,
        location = ConvertGeoDB2DTO(dbObj.location)
      };

      return retVal;
    }

    static private GeometryDTO ConvertGeoDB2DTO(GeoJsonGeometry<GeoJson2DCoordinates> location)
    {
      if (location == null)
      { return null; }

      GeometryDTO ret = null;

      if (location is GeoJsonPoint<GeoJson2DCoordinates> point)
      {
        ret = new GeometryCircleDTO(
          new Geo2DCoordDTO() { point.Coordinates.Y, point.Coordinates.X }
        );
      }

      if (location is GeoJsonPolygon<GeoJson2DCoordinates> polygon)
      {
        List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();
        var retPolygon = new GeometryPolygonDTO();

        foreach (var cur in polygon.Coordinates.Exterior.Positions)
        {
          retPolygon.coord.Add(new Geo2DCoordDTO() { cur.Y, cur.X });
        }

        if (retPolygon.coord.Count > 3)
        {
          retPolygon.coord.RemoveAt(retPolygon.coord.Count - 1);
        }
        ret = retPolygon;
      }

      if (location is GeoJsonLineString<GeoJson2DCoordinates> line)
      {
        var retLine = new GeometryPolylineDTO();
        List<GeoJson2DCoordinates> coordinates = new List<GeoJson2DCoordinates>();

        foreach (var cur in line.Coordinates.Positions)
        {
          retLine.coord.Add(new Geo2DCoordDTO() { cur.Y, cur.X });
        }
        ret = retLine;
      }

      ret.type = location.Type.ToString();
      return ret;
    }

    public static List<DBObjExtraProperty> ConvertExtraPropsToDB(List<ObjExtraPropertyDTO> extra_props)
    {
      if (extra_props == null)
      { return null; }

      var ep_db = new List<DBObjExtraProperty>();
      var propertieNames = typeof(FigureZoomedDTO).GetProperties().Select(x => x.Name).ToList();

      propertieNames.AddRange(
        typeof(FigureGeoDTO).GetProperties().Select(x => x.Name)
        );


      foreach (var prop in extra_props)
      {
        // "radius", "zoom_level"
        if (propertieNames.Contains(prop.prop_name))
        {
          continue;
        }

        DBObjExtraProperty newProp = new DBObjExtraProperty()
        {
          prop_name = prop.prop_name,
          visual_type = prop.visual_type
        };
        var s = BsonType.Double.ToString();
        if (prop.visual_type == BsonType.Double.ToString())
        {
          if(double.TryParse(
            prop.str_val,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var result))
          {
            newProp.str_val = new BsonDocument(
            "str_val",
            result
            );
          }
         }
        else
        if (prop.visual_type == BsonType.DateTime.ToString())
        {
          newProp.str_val = new BsonDocument(
            "str_val",
            DateTime
              .Parse(prop.str_val)
              .ToUniversalTime()
            );
        }

        if (newProp.str_val == null)
        {
          newProp.str_val = new BsonDocument(
            "str_val",
            prop.str_val
            );
        }
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
        return "__clr";
      }
      return prop.visual_type;
    }
    public static List<ObjExtraPropertyDTO> ConverDBExtraProp2DTO(List<DBObjExtraProperty> props)
    {
      var retVal = new List<ObjExtraPropertyDTO> ();

      if (props == null)
      {
        return retVal;
      }

      foreach (var prop in props)
      {
        ObjExtraPropertyDTO newProp = new ObjExtraPropertyDTO()
        {
          prop_name = prop.prop_name,
          str_val = prop.str_val.GetValue("str_val", string.Empty).ToString(),
          visual_type = GetDefaultVisualType(prop)
        };
        retVal.Add(newProp);
      }

      return retVal;
    }
    public static ObjPropsDTO Conver2Property2DTO(DBMarkerProperties props)
    {
      if (props == null)
      {
        return null;
      }

      ObjPropsDTO mProps = new ObjPropsDTO()
      {
        extra_props = ConverDBExtraProp2DTO(props.extra_props),
        id = props.id
      };

      return mProps;
    }
  }
}
