using DbLayer;
using Domain;
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
    public static MarkerDTO GetMarkerDTO(Marker marker)
    {
      return new MarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static TreeMarkerDTO GetTreeMarkerDTO(Marker marker)
    {
      return new TreeMarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static ObjPropsDTO GetObjPropsDTO(Marker marker)
    {
      return new ObjPropsDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }
    public static Marker ConvertObjPropsDTOToMarker(ObjPropsDTO marker)
    {
      return new Marker()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static dynamic ConvertGeoPoint2DTO(GeoPoint geoPart)
    {
      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Point)
      {
        var figure = new FigureCircleDTO();
        var pt = geoPart.location as GeoJsonPoint<GeoJson2DCoordinates>;
        figure.geometry = new double[2] { pt.Coordinates.Y, pt.Coordinates.X };
        return figure;
      }

      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Polygon)
      {
        var figure = new FigurePolygonDTO();

        var pt = geoPart.location as GeoJsonPolygon<GeoJson2DCoordinates>;

        List<double[]> list = new List<double[]>();

        foreach (var cur in pt.Coordinates.Exterior.Positions)
        {
          list.Add(new double[2] { cur.Y, cur.X });
        }

        if (list.Count > 3)
        {
          list.RemoveAt(list.Count - 1);
        }

        figure.geometry = list.ToArray();
        return figure;
      }

      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.LineString)
      {
        var figure = new FigurePolylineDTO();

        var pt = geoPart.location as GeoJsonLineString<GeoJson2DCoordinates>;

        List<double[]> list = new List<double[]>();

        foreach (var cur in pt.Coordinates.Positions)
        {
          list.Add(new double[2] { cur.Y, cur.X });
        }
        figure.geometry = list.ToArray();
        return figure;
      }

      return null;
    }

    public static MarkerProperties ConvertDTO2Property(ObjPropsDTO props)
    {
      MarkerProperties mProps = new MarkerProperties()
      {
        extra_props = new List<ObjExtraProperty>(),
        id = props.id
      };

      foreach (var prop in props.extra_props)
      {
        ObjExtraProperty newProp = new ObjExtraProperty()
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
    public static ObjPropsDTO Conver2Property2DTO(MarkerProperties props)
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
