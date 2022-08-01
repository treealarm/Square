using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.StateWebSock;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public class TrackService
  {
    private readonly IMongoCollection<DBTrackPoint> _collection;
    public TrackService(IMongoCollection<DBTrackPoint> collection)
    {
      _collection = collection;
    }

    public async Task InsertManyAsync(List<TrackPointDTO> newObjs)
    {
      List<DBTrackPoint> list = new List<DBTrackPoint>();

      foreach (var track in newObjs)
      {
        var dbTrack = new DBTrackPoint()
        {
          timestamp = track.timestamp,
          figure = ConvertDTO2DB(track.figure)
        };
        list.Add(dbTrack);
      }
      await _collection.InsertManyAsync(list);
    }

    private DBGeoObject ConvertDTO2DB(GeoObjectDTO dto)
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

    private GeoJsonGeometry<GeoJson2DCoordinates> ConvertGeoDTO2DB(GeometryDTO location)
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
  }
}
