using Domain;
using Domain.GeoDBDTO;
using GeoJSON.Net;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPKGConvertor
{
  internal class Convertor
  {
    private volatile bool _configured;
    private volatile string _geopackagePath;
    private volatile DataSource _dataSource;
    private static int _uid = 10000;
    public bool Configure(string geopackagePath)
    {
      if (_configured)
        return true;

      if (!File.Exists(geopackagePath))
      {
        Console.WriteLine("geopackage path: " + geopackagePath + " does not exist!");
        return false;
      }

      GdalConfiguration.ConfigureGdal();
      GdalConfiguration.ConfigureOgr();

      Gdal.AllRegister();
      Ogr.RegisterAll();

      _geopackagePath = geopackagePath;

      _dataSource = Ogr.Open(_geopackagePath, 0);

      if (_dataSource == null)
      {
        Console.WriteLine("failed to open geopackage file!");
        return false;
      }

      _configured = true;

      return true;
    }

    public List<JObject> ReadGeometry(string file)
    {
      var file_id = Path.GetFileNameWithoutExtension(file).ToLower();

      List<JObject> list = new List<JObject>();

      if (file_id != "forbidden_zones" &&
        file_id != "parkings")
      {
        return list;
      }
      var dir = Path.GetDirectoryName(file);

      var mapRoadIndex = new Dictionary<string, string>();
      var mapRoadPop = new Dictionary<string, string>();
      var mapRoadSpeed = new Dictionary<string, string>();

      if (file_id == "hexes")
      {
        var csv = Path.Combine(dir, "road_index.csv");
        var lines = File.ReadAllLines(csv);

        foreach (var line in lines)
        {
          var row = line.Split(',');
          mapRoadIndex[row[0]] = row[1];
        }

        csv = Path.Combine(dir, "routes_hex20m.csv");
        lines = File.ReadAllLines(csv);

        foreach (var line in lines)
        {
          var row = line.Split(',');
          mapRoadPop[row[1]] = row[0];
        }

        csv = Path.Combine(dir, "speed_median_hex20m_hackaton.csv");
        lines = File.ReadAllLines(csv);

        foreach (var line in lines)
        {
          var row = line.Split(',');
          mapRoadSpeed[row[1]] = row[0];
        }

        csv = Path.Combine(dir, "clashes.csv");
        lines = File.ReadAllLines(csv);
      }    

      var layerCount = _dataSource.GetLayerCount();

      for (int l = 0; l < layerCount; l++)
      {
        var resultLayer = _dataSource.GetLayerByIndex(l);

        if (resultLayer == null)
          continue;

        string geoCol = resultLayer.GetGeometryColumn();

        if (string.IsNullOrEmpty(geoCol))
        {
          continue;
        }

        //String sql = $"SELECT * FROM {table}";

        //Layer resultLayer = _dataSource.ExecuteSQL(sql, null, "");
        

        var count = resultLayer.GetFeatureCount(0);

        if (count == 0)
        {
          continue;
        }

        Feature feature = resultLayer.GetNextFeature();


        while (feature != null)
        {
          Geometry geometry = feature.GetGeometryRef();

          var id = feature.GetFieldAsString("id");
          string[] options = new string[1];
          var s = geometry.ExportToJson(options);

          JObject o = JObject.Parse(s);

          if (mapRoadIndex.TryGetValue(id, out var map))
          {
            o.AddFirst(new JProperty("ri_median", map));
          }

          if (mapRoadPop.TryGetValue(id, out var map1))
          {
            o.AddFirst(new JProperty("route_sum", map1));
          }

          if (mapRoadSpeed.TryGetValue(id, out var map2))
          {
            o.AddFirst(new JProperty("speed_median", map2));
          }

          o.AddFirst(new JProperty("src_id", id));
          o.AddFirst(new JProperty("layer_name", $"{resultLayer.GetName()}"));
          
          list.Add(o);
          feature = resultLayer.GetNextFeature();
        }
      }

      return list;
    }

    public void AddJListToFigs(string curFile, List<JObject> list, FiguresDTO figDto)
    {
      FigureGeoDTO fig = new FigureGeoDTO()
      {
        name = curFile,
        id = _uid++.ToString().PadLeft(24, '0')
      };
      figDto.figs.Add(fig);
      string zoom = null;

      if (list.Count > 1000)
      {
        zoom = "13-18";
      }
      if (list.Count > 10000)
      {
        zoom = "15-18";
      }

      Color color = Color.FromArgb(DateTime.Now.Second * 1000);
      int nColorWin32 = ColorTranslator.ToWin32(color);
      var text = string.Format("{0:X6}", nColorWin32);

      foreach (var item in list)
      {
        AddJObjToFigs(fig.id, item, figDto, zoom, $"#{text}");
      }
    }

    public void AddJObjToFigs(
      string parent_id,
      JObject jObject,
      FiguresDTO figDto,
      string zoom,
      string color
     )
    {
      GeometryDTO geo = null;
      var deserialized = JsonConvert.DeserializeObject<GeoJSONObject>(jObject?.ToString(), new GeoJsonConverter());

      if (deserialized.Type == GeoJSONObjectType.Polygon)
      {
        var figVal = new GeometryPolygonDTO();
        figVal.coord = new List<Geo2DCoordDTO> ();
        geo = figVal;

        var des = deserialized as Polygon;

        foreach (var item in des.Coordinates) {
          foreach(var coord in item.Coordinates)
          {
            figVal.coord.Add(new Geo2DCoordDTO()
            {
              Lon = coord.Longitude,
              Lat = coord.Latitude
            });
          }          
        }
      }
      else
      if (deserialized.Type == GeoJSONObjectType.Point)
      {
        var figVal = new GeometryCircleDTO();
        geo = figVal;

        var des = deserialized as GeoJSON.Net.Geometry.Point;

        var coord = des.Coordinates;
        {
            figVal.coord = new Geo2DCoordDTO()
            {
              Lon = coord.Longitude,
              Lat = coord.Latitude
            };
        }
      }
      else
      if (deserialized.Type == GeoJSONObjectType.LineString)
      {
        var figVal = new GeometryPolylineDTO();
        figVal.coord = new List<Geo2DCoordDTO>();
        geo = figVal;

        var des = deserialized as LineString;

        var ls = des.Coordinates;
        foreach(var coord in ls)
        {
          figVal.coord.Add(new Geo2DCoordDTO()
          {
            Lon = coord.Longitude,
            Lat = coord.Latitude
          });
        }
      }
      else
      {
        Console.WriteLine($"unknown type:{deserialized.Type.ToString()}");
      }

      FigureGeoDTO fig = new FigureGeoDTO()
      {
        name = jObject.GetValue("src_id")?.ToString(),
        parent_id = parent_id,
        geometry = geo,
        id = _uid++.ToString().PadLeft(24, '0'),
        zoom_level = zoom
      };

      fig.extra_props = new List<ObjExtraPropertyDTO>();

      fig.extra_props.Add(new ObjExtraPropertyDTO() { 
      prop_name = "layer_name",
      str_val = jObject.GetValue("layer_name")?.ToString()
      });

      fig.extra_props.Add(new ObjExtraPropertyDTO()
      {
        prop_name = "src_id",
        str_val = jObject.GetValue("src_id")?.ToString()
      });

      string[] other_params = { "speed_median", "route_sum", "ri_median" };

      var curColor = color;

      foreach (var param in other_params)
      {
        var paramVal = jObject.GetValue(param);

        if ( paramVal == null)
        {
          continue;
        }

        if (param == "ri_median" && Double.TryParse(
          paramVal.ToString(),
          NumberStyles.Any,
          CultureInfo.InvariantCulture,
          out var result))
        {
          Color clr = Color.FromArgb(254, Math.Min((int)(result * 254), 254), 128,128);
          int nColorWin32 = ColorTranslator.ToWin32(clr);
          curColor = $"#{((int)(result * 254)).ToString("X2")}AAAA";
        }

        fig.extra_props.Add(new ObjExtraPropertyDTO()
        {
          prop_name = param,
          str_val = paramVal.ToString(),
          visual_type = "Double"
        });
      }

      fig.extra_props.Add(new ObjExtraPropertyDTO()
      {
        prop_name = "color",
        str_val = curColor
      });

      figDto.figs.Add(fig);
    }
  }
}
