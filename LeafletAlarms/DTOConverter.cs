using DbLayer;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeafletAlarms
{
  public class DTOConverter
  {
    public static MarkerDTO GetMarkerDTO(DBMarker marker)
    {
      return new MarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static TreeMarkerDTO GetTreeMarkerDTO(DBMarker marker)
    {
      return new TreeMarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static ObjPropsDTO GetObjPropsDTO(DBMarker marker)
    {
      return new ObjPropsDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }
    public static DBMarker ConvertObjPropsDTOToMarker(ObjPropsDTO marker)
    {
      return new DBMarker()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static dynamic ConvertGeoPoint2DTO(DBGeoObject geoPart)
    {
      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Point)
      {
        var figure = new FigureCircleDTO();
        var pt = geoPart.location as GeoJsonPoint<GeoJson2DCoordinates>;
        figure.geometry = 
          new GeometryCircleDTO(new Geo2DCoordDTO() { pt.Coordinates.Y, pt.Coordinates.X });
        return figure;
      }

      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Polygon)
      {
        var figure = new FigurePolygonDTO();

        var pt = geoPart.location as GeoJsonPolygon<GeoJson2DCoordinates>;

        GeometryPolygonDTO list = new GeometryPolygonDTO();

        foreach (var cur in pt.Coordinates.Exterior.Positions)
        {
          list.coord.Add(new Geo2DCoordDTO() { cur.Y, cur.X });
        }

        if (list.coord.Count > 3)
        {
          list.coord.RemoveAt(list.coord.Count - 1);
        }

        figure.geometry = list;
        return figure;
      }

      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.LineString)
      {
        var figure = new FigurePolylineDTO();

        var pt = geoPart.location as GeoJsonLineString<GeoJson2DCoordinates>;

        GeometryPolylineDTO list = new GeometryPolylineDTO();

        foreach (var cur in pt.Coordinates.Positions)
        {
          list.coord.Add(new Geo2DCoordDTO() { cur.Y, cur.X });
        }
        figure.geometry = list;
        return figure;
      }

      return null;
    }

    public static DBMarkerProperties ConvertDTO2Property(ObjPropsDTO props)
    {
      DBMarkerProperties mProps = new DBMarkerProperties()
      {
        extra_props = new List<DBObjExtraProperty>(),
        id = props.id
      };

      if (props.extra_props == null)
      {
        return mProps;
      }

      var propertieNames = typeof(FigureZoomedDTO).GetProperties().Select(x => x.Name).ToList();

      propertieNames.AddRange(
        typeof(FigureCircleDTO).GetProperties().Select(x => x.Name)
        );


      foreach (var prop in props.extra_props)
      {
        // "radius", "min_zoom", "max_zoom"
        if (propertieNames.Contains(prop.prop_name))
        {
          continue;
        }

        DBObjExtraProperty newProp = new DBObjExtraProperty()
        {
          prop_name = prop.prop_name,          
          visual_type = prop.visual_type
        };

        if (prop.visual_type == BsonType.DateTime.ToString())
        {
          newProp.MetaValue = new BsonDocument(
            "str_val",
            DateTime.Parse(prop.str_val)
            );
        }
        else
        {
          newProp.MetaValue = new BsonDocument(
            "str_val",
            prop.str_val
            );
        }
        mProps.extra_props.Add(newProp);
      }

      return mProps;
    }
    public static ObjPropsDTO Conver2Property2DTO(DBMarkerProperties props)
    {
      if (props == null)
      {
        return null;
      }

      ObjPropsDTO mProps = new ObjPropsDTO()
      {
        extra_props = new List<ObjExtraPropertyDTO>(),
        id = props.id
      };

      foreach (var prop in props.extra_props)
      {
        ObjExtraPropertyDTO newProp = new ObjExtraPropertyDTO()
        {
          prop_name = prop.prop_name,
          str_val = prop.MetaValue.GetValue("str_val", string.Empty).ToString(),
          visual_type = prop.visual_type
        };
        mProps.extra_props.Add(newProp);
      }

      return mProps;
    }
  }
}
