using Domain;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;

namespace DbLayer.Services
{
  internal class TrackService : ITrackService
  {
    private readonly PgDbContext _dbContext;
    private readonly ILevelService _levelService;

    public TrackService(PgDbContext dbContext, ILevelService levelService)
    {
      _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
      _levelService = levelService ?? throw new ArgumentNullException(nameof(levelService));
    }


    static public Geometry ConvertGeoDTO2DB(GeometryDTO location, GeometryFactory factory = null)
    {
      if (location == null) return null;

      Geometry geom = null;
      if (factory == null)
        factory = new GeometryFactory(new PrecisionModel(), 4326); // 4326 = WGS84

      switch (location)
      {
        case GeometryCircleDTO point:
          geom = factory.CreatePoint(new Coordinate(point.coord.Lon, point.coord.Lat));
          
          break;

        case GeometryPolygonDTO polygon:
          var coordsPoly = polygon.coord
              .Select(c => new Coordinate(c[1], c[0]))
              .ToList();

          // замыкаем полигон
          coordsPoly.Add(coordsPoly[0]);

          geom = factory.CreatePolygon(coordsPoly.ToArray());
          break;

        case GeometryPolylineDTO line:
          var coordsLine = line.coord
              .Select(c => new Coordinate(c[1], c[0]))
              .ToArray();

          geom = factory.CreateLineString(coordsLine);
          break;
      }
      geom.SRID = 4326;
      return geom;
    }


    public static string ConvertExtraPropsToJsonString(List<ObjExtraPropertyDTO> extra_props)
    {
      if (extra_props == null || extra_props.Count == 0)
        return null;

      var propertyNames = typeof(FigureZoomedDTO).GetProperties().Select(x => x.Name).ToList();
      propertyNames.AddRange(typeof(FigureGeoDTO).GetProperties().Select(x => x.Name));

      var jsonList = new List<Dictionary<string, string>>();

      foreach (var prop in extra_props)
      {
        if (propertyNames.Contains(prop.prop_name))
          continue;

        string value = prop.str_val;

        if (prop.visual_type == VisualTypes.Double &&
            double.TryParse(prop.str_val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
        {
          value = d.ToString(CultureInfo.InvariantCulture);
        }
        else if (prop.visual_type == VisualTypes.DateTime &&
                 DateTime.TryParse(prop.str_val, out var dt))
        {
          value = dt.ToUniversalTime().ToString("o"); // ISO 8601
        }

        jsonList.Add(new Dictionary<string, string>
        {
          ["prop_name"] = prop.prop_name,
          ["str_val"] = value,
          ["visual_type"] = prop.visual_type
        });
      }

      return JsonSerializer.Serialize(jsonList);
    }

    public async Task<List<TrackPointDTO>> InsertManyAsync(List<TrackPointDTO> newObjs)
    {
      if (newObjs == null || newObjs.Count == 0)
        return new List<TrackPointDTO>();

      var list = new List<DBTrackPoint>();

      foreach (var track in newObjs)
      {
        var dbTrack = new DBTrackPoint()
        {
          id = Domain.Utils.NewGuid(),
          object_id = Domain.Utils.ConvertObjectIdToGuid(track.id),
          timestamp = track.timestamp ?? DateTime.UtcNow,
          figure = ConvertGeoDTO2DB(track.figure.location), // возвращает Geometry
          radius = track.figure?.radius,
          zoom_level = track.figure?.zoom_level,
          extra_props = ConvertExtraPropsToJsonString(track.extra_props) // возвращает JsonDocument
        };

        list.Add(dbTrack);
      }

      if (list.Count > 0)
      {
        await _dbContext.Tracks.AddRangeAsync(list);
        await _dbContext.SaveChangesAsync();
      }

      // Конвертация обратно в DTO
      var result = new List<TrackPointDTO>();
      foreach (var t in list)
      {
        result.Add(ConvertDB2TrackPointDTO(t));
      }

      return result;
    }

    public static List<ObjExtraPropertyDTO> ConvertJsonStringToExtraProps(string json)
    {
      if (string.IsNullOrEmpty(json))
        return new List<ObjExtraPropertyDTO>();

      return JsonSerializer.Deserialize<List<ObjExtraPropertyDTO>>(json);
    }

    static public GeoObjectDTO ConvertDB2DTO(DBTrackPoint dbObj)
    {
      if (dbObj == null)
        return null;

      var retVal = new GeoObjectDTO()
      {
        id = Domain.Utils.ConvertGuidToObjectId(dbObj.object_id ?? Guid.Empty),
        radius = dbObj.radius,
        zoom_level = dbObj.zoom_level,
        location = ConvertGeoDB2DTO(dbObj.figure) // Geometry → GeometryDTO
      };

      return retVal;
    }

    static private GeometryDTO ConvertGeoDB2DTO(Geometry geom)
    {
      if (geom == null)
        return null;

      GeometryDTO ret = null;

      switch (geom)
      {
        case Point point:
          ret = new GeometryCircleDTO(
              new Geo2DCoordDTO { Lat = point.Y, Lon = point.X });
          break;

        case Polygon polygon:
          var retPolygon = new GeometryPolygonDTO();
          var coords = polygon.ExteriorRing.Coordinates;
          foreach (var c in coords)
          {
            retPolygon.coord.Add(new Geo2DCoordDTO { Lat = c.Y, Lon = c.X });
          }
          // удаляем замкнутую последнюю точку
          if (retPolygon.coord.Count > 3)
            retPolygon.coord.RemoveAt(retPolygon.coord.Count - 1);

          ret = retPolygon;
          break;

        case LineString line:
          var retLine = new GeometryPolylineDTO();
          foreach (var c in line.Coordinates)
          {
            retLine.coord.Add(new Geo2DCoordDTO { Lat = c.Y, Lon = c.X });
          }
          ret = retLine;
          break;

        default:
          throw new NotSupportedException($"Geometry type {geom.GeometryType} not supported");
      }

      ret.type = geom.GeometryType; // Point, Polygon, LineString
      return ret;
    }


    private TrackPointDTO ConvertDB2TrackPointDTO(DBTrackPoint t)
    {
      if (t == null)
        return null;

      var dto = new TrackPointDTO()
      {
        id = Domain.Utils.ConvertGuidToObjectId(t.object_id),
        timestamp = t.timestamp,
        figure = ConvertDB2DTO(t), // Geometry → DTO
        extra_props = ConvertJsonStringToExtraProps(t.extra_props) // string → List<ObjExtraPropertyDTO>
      };

      return dto;
    }


    public async Task<TrackPointDTO> GetLastAsync(string figure_id, DateTime beforeTime)
    {
      if (string.IsNullOrEmpty(figure_id))
        return null;

      // Поиск по фигуре и времени
      var dbTrack = await _dbContext.Tracks
          .Where(t => t.figure != null && t.figure is Geometry && t.timestamp < beforeTime)
          // Для поиска по id фигуры, если храним в DBGeoObject.id:
          .Where(t => t.object_id == Domain.Utils.ConvertObjectIdToGuid(figure_id))
          .OrderByDescending(t => t.timestamp)
          .FirstOrDefaultAsync();

      return ConvertDB2TrackPointDTO(dbTrack);
    }


    public async Task<TrackPointDTO> GetByIdAsync(string id)
    {
      if (string.IsNullOrEmpty(id))
        return null;

      if (!Guid.TryParse(id, out var guid))
        return null;

      var dbTrack = await _dbContext.Tracks
          .Where(t => t.id == guid)
          .FirstOrDefaultAsync();

      return ConvertDB2TrackPointDTO(dbTrack);
    }

    private List<TrackPointDTO> DBListToDTO(List<DBTrackPoint> dbTracks)
    {
      List<TrackPointDTO> list = new List<TrackPointDTO>();
      foreach (var t in dbTracks)
      {
        var dto = ConvertDB2TrackPointDTO(t);
        list.Add(dto);
      }
      return list;
    }

    public async Task<List<TrackPointDTO>> GetFirstTracksByTime(
        DateTime? time_start,
        DateTime? time_end,
        List<string> figIds
    )
    {
      IQueryable<DBTrackPoint> query = _dbContext.Tracks;

      // фильтр по времени
      if (time_start.HasValue && time_end.HasValue)
      {
        query = query.Where(t => t.timestamp >= time_start.Value && t.timestamp <= time_end.Value);
      }

      // фильтр по object_id (ранее meta.figure.id)
      if (figIds != null && figIds.Count > 0)
      {
        var guidIds = figIds
            .Select(f => Domain.Utils.ConvertObjectIdToGuid(f))
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

        if (guidIds.Count > 0)
        {
          query = query.Where(t => t.object_id.HasValue && guidIds.Contains(t.object_id.Value));
        }
      }

      // ограничение количества
      var dbTracks = await query
          .OrderBy(t => t.timestamp) // можно менять сортировку
          .Take(10000)
          .ToListAsync();

      // конвертация в DTO
      return dbTracks.Select(ConvertDB2TrackPointDTO).ToList();
    }


    private async Task<List<TrackPointDTO>> DoGetTracksByBox(BoxTrackDTO box)
    {
      int limit = box.count > 0 ? box.count.Value : 10000;
      IQueryable<DBTrackPoint> query = _dbContext.Tracks;

      // Гео-фильтр
      Geometry geometry = null;
      var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

      if (box.zone != null && box.zone.Count > 0)
      {
        var polygons = box.zone
        .Select(z => ConvertGeoDTO2DB(z, geometryFactory))
        .Where(g => g != null)
        .ToList();

        // объединяем все зоны через OR
        query = query.Where(t => polygons.Any(p => t.figure != null && t.figure.Intersects(p)));
      }
      else
      {
        // создаём прямоугольник по wn/es
        var rect = geometryFactory.CreatePolygon(new[]
        {
            new Coordinate(box.wn[0], box.wn[1]),
            new Coordinate(box.es[0], box.wn[1]),
            new Coordinate(box.es[0], box.es[1]),
            new Coordinate(box.wn[0], box.es[1]),
            new Coordinate(box.wn[0], box.wn[1])
        });
        query = query.Where(t => t.figure != null && t.figure.Intersects(rect));
      }

      if (box.not_in_zone)
      {
        query = query.Where(t => !t.figure.Intersects(geometry));
      }

      // фильтр по zoom
      if (box.zoom != null)
      {
        var levels = await _levelService.GetLevelsByZoom(box.zoom);
        query = query.Where(t => levels.Contains(t.zoom_level) || string.IsNullOrEmpty(t.zoom_level));
      }

      // фильтр по времени
      if (box.time_start.HasValue && box.time_end.HasValue)
      {
        query = query.Where(t => t.timestamp >= box.time_start.Value && t.timestamp <= box.time_end.Value);
      }

      // фильтр по объектам/figure ids
      if (box.ids != null && box.ids.Count > 0)
      {
        var guidIds = box.ids
            .Select(Domain.Utils.ConvertObjectIdToGuid)
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

        query = query.Where(t => t.object_id.HasValue && guidIds.Contains(t.object_id.Value));
      }

      // фильтр по extra_props jsonb
      if (box.property_filter != null && box.property_filter.props.Count > 0)
      {
        foreach (var prop in box.property_filter.props)
        {
          query = query.Where(t =>
              EF.Functions.JsonContains(t.extra_props,
                  $"[{{\"prop_name\":\"{prop.prop_name}\",\"str_val\":\"{prop.str_val}\"}}]"));
        }
      }

      // сортировка
      if (box.sort < 0)
        query = query.OrderByDescending(t => t.timestamp);
      else
        query = query.OrderBy(t => t.timestamp);

      // лимит
      var dbTracks = await query.Take(limit).ToListAsync();

      return dbTracks.Select(ConvertDB2TrackPointDTO).ToList();
    }


    public async Task<List<TrackPointDTO>> GetTracksByFilter(SearchFilterDTO filter_in)
    {
      int limit = filter_in.count > 0 ? filter_in.count : 10000;

      // базовый запрос по времени
      var query = _dbContext.Tracks
          .Where(t => t.timestamp >= filter_in.time_start &&
                      t.timestamp <= filter_in.time_end);

      // Пагинация по start_id
      if (!string.IsNullOrEmpty(filter_in.start_id))
      {
        if (Guid.TryParse(filter_in.start_id, out var startGuid))
        {
          if (filter_in.forward > 0)
            query = query.Where(t => t.id.CompareTo(startGuid) > 0);
          else
            query = query.Where(t => t.id.CompareTo(startGuid) < 0);
        }
      }

      // Фильтр по extra_props (jsonb)
      if (filter_in.property_filter != null && filter_in.property_filter.props.Count > 0)
      {
        foreach (var prop in filter_in.property_filter.props)
        {
          var filterJson = JsonSerializer.Serialize(new { prop_name = prop.prop_name, str_val = prop.str_val });
          query = query.Where(t => EF.Functions.JsonContains(t.extra_props, filterJson));
        }
      }

      var dbObjects = await query
          .OrderBy(t => t.timestamp) // или .OrderByDescending если нужно
          .Take(limit)
          .ToListAsync();

      return dbObjects.Select(ConvertDB2TrackPointDTO).ToList();
    }

    public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
    {
      if (
        box.time_start == null &&
        box.time_end == null
      )
      {
        // We do not search without time diapason.
        return new List<TrackPointDTO>();
      }

      //await AddIdsByProperties(box);

      var trackPoints = await DoGetTracksByBox(box);

      if (box.sort < 0)
      {
        trackPoints = trackPoints.OrderByDescending(f => f.timestamp).ToList();
      }

      return trackPoints;
    }
  }
}
