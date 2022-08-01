using Domain.GeoDBDTO;
using Domain.GeoDTO;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
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

    static public GeoJsonGeometry<GeoJson2DCoordinates> ConvertGeoDTO2DB(GeometryDTO location)
    {
      GeoJsonGeometry<GeoJson2DCoordinates> ret = null;

      if (location is GeometryCircleDTO point)
      {
        ret = new GeoJsonPoint<GeoJson2DCoordinates>(
          GeoJson.Position(point.coord[1], point.coord[0])
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
      return ret;
    }
  }
}
