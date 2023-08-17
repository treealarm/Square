using DbLayer;
using Domain;
using Domain.GeoDBDTO;
using MongoDB.Driver.GeoJsonObjectModel;

namespace LeafletAlarms
{
  public class DTOConverter
  {
    public static MarkerDTO GetMarkerDTO(BaseMarkerDTO marker)
    {
      return new MarkerDTO()
      {
        id = marker.id,
        name = marker.name,
        parent_id = marker.parent_id
      };
    }

    public static ObjPropsDTO GetObjPropsDTO(BaseMarkerDTO marker)
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
        parent_id = marker.parent_id,
        external_type = marker.external_type
      };
    }

    public static dynamic ConvertGeoPoint2DTO(DBGeoObject geoPart)
    {
      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Point)
      {
        var figure = new FigureGeoDTO();
        var pt = geoPart.location as GeoJsonPoint<GeoJson2DCoordinates>;
        figure.geometry = 
          new GeometryCircleDTO(new Geo2DCoordDTO() { pt.Coordinates.Y, pt.Coordinates.X });
        return figure;
      }

      if (geoPart.location.Type == MongoDB.Driver.GeoJsonObjectModel.GeoJsonObjectType.Polygon)
      {
        var figure = new FigureGeoDTO();

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
        var figure = new FigureGeoDTO();

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
  }
}
