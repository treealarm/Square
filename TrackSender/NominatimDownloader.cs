using Domain.GeoDBDTO;
using Domain;
using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TrackSender.Models;
using Itinero.LocalGeo;

namespace TrackSender
{
  internal class NominatimProcessor
  {
    //https://wiki.openstreetmap.org/wiki/RU:%D0%9C%D0%BE%D1%81%D0%BA%D0%B2%D0%B0/%D0%90%D0%B4%D0%BC%D0%B8%D0%BD%D0%B8%D1%81%D1%82%D1%80%D0%B0%D1%82%D0%B8%D0%B2%D0%BD%D0%BE-%D1%82%D0%B5%D1%80%D1%80%D0%B8%D1%82%D0%BE%D1%80%D0%B8%D0%B0%D0%BB%D1%8C%D0%BD%D0%BE%D0%B5_%D0%B4%D0%B5%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5
    //childeren of moscow:
    //https://nominatim.openstreetmap.org/details?osmtype=R&osmid=102269&addressdetails=1&hierarchy=1&group_hierarchy=1&format=json&pretty=1
    //https://nominatim.openstreetmap.org/details.php?osmtype=R&osmid=102269&addressdetails=1&hierarchy=0&group_hierarchy=1&format=json
    //https://nominatim.openstreetmap.org/details?place_id=337939658&format=json&pretty=1&hierarchy=1
    //https://nominatim.openstreetmap.org/details?place_id=337939658&format=json&pretty=1

    HttpClient _client = new HttpClient();
   
    public async Task RunAsync(CancellationToken token, List<Task> tasks)
    {
      _client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
      _client.DefaultRequestHeaders.Accept.Clear();
      _client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));

      _client.DefaultRequestHeaders.UserAgent.Clear();
      _client
        .DefaultRequestHeaders
        .UserAgent
        .Add(
          new ProductInfoHeaderValue(
            "f1ana.Nominatim.API",
            Assembly.GetExecutingAssembly().GetName().Version.ToString()));
      //await SaveMoscowToLocalDisk();
      //await SaveMKadLocalDisk();

      var mkad = await GetMkadPolyline();
    }

    static async Task<List<int>> GetMkadOsmIds()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = $"TrackSender.MKAD.MKAD.xml";

      string s = string.Empty;
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      using (StreamReader reader = new StreamReader(stream))
      {
        s = await reader.ReadToEndAsync();
      }

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(s);

      XmlNode node = doc.SelectSingleNode("osm/relation");

      XmlNodeList prop = node.SelectNodes("member");

      List<int> osmIds = new List<int>();

      foreach (XmlNode item in prop)
      {
        var curAttr = item.Attributes["ref"];

        if (curAttr != null)
        {
          osmIds.Add(int.Parse(curAttr.Value));
        }
      }

      return osmIds;
    }
    private async Task SaveMKadLocalDisk()
    {
      string s;
      var osmIds = await GetMkadOsmIds();

      foreach (var osmid in osmIds)
      {
        string filename = $"{osmid}.json";
        filename = Path.Combine(App.Default.LocalPathMkad, filename);

        if (File.Exists(filename))
        {
          continue;
        }

        s = await GetByOsmId(osmid, "W");

        if (s == null)
        {
          continue;
        }
        await File.WriteAllTextAsync(filename, s);
      }
    }

    private async Task SaveMoscowToLocalDisk()
    {
      foreach (var osmid in MoscowOsm.osmids)
      {
        string filename = $"{osmid[0]}.json";
        filename = Path.Combine(App.Default.LocalPathPolygonMoscow, filename);

        if (File.Exists(filename))
        {
          continue;
        }

        var s = await GetByOsmId(osmid[0], "R");

        if (s == null)
        {
          continue;
        }
        await File.WriteAllTextAsync(
          filename, s);
      }
    }

    public async Task<string> GetByOsmId(int osmid, string osmtype)
    {
      try
      {
        var request = $"details?osmtype={osmtype}&osmid={osmid}&polygon_geojson=1&format=json&pretty=1";
        HttpResponseMessage response =
          await _client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        var s = await response.Content.ReadAsStringAsync();

        return s;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return null;

    }

    public static async Task<List<FigureGeoDTO>> GetMkadPolyline()
    {
      var result = new List<FigureGeoDTO>();

      var figure = new FigureGeoDTO()
      {
        name = "MKAD",
        zoom_level = "13",
        geometry = new GeometryPolylineDTO()
        {
          coord = new List<Geo2DCoordDTO>()
        }
      };

      figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "mkad",
                str_val = "true"
              }
            };
      result.Add(figure);

      string s;
      var osmIds = await GetMkadOsmIds();

      foreach (var osmid in osmIds)
      {
        var geoObj = await NominatimProcessor.GetOsmFigureFromDisk(osmid, "MKAD");

        if (geoObj == null)
        {
          continue;
        }
        else
        {
          if (geoObj.geometry.type == "LineString" || geoObj.geometry.type == "Point")
          {
            CoordinateList coords;

            if (geoObj.geometry.type == "LineString")
            {
              coords =
                JsonSerializer.Deserialize<CoordinateList>(geoObj.geometry.coordinates.ToString());

            }
            else
            {
              var point =
                JsonSerializer.Deserialize<Geo2DCoordDTO>(geoObj.geometry.coordinates.ToString());
              coords = new CoordinateList()
              {
                point
              };
            }

            var nameOfpolygon = $"{geoObj.names.name}";

            foreach (var coord in coords)
            {
              var temp = coord.X;
              coord.X = coord.Y;
              coord.Y = temp;
            }

            var geometry = figure.geometry as GeometryPolylineDTO;
            geometry.coord.AddRange(coords);
          }
          else
          {
            throw new Exception($"Bad MKAD json {osmid}");
            // Undefined.
          }
        }
      }

      return result;
    }

    public static async Task<Root> GetOsmFigureFromDisk(int osmid, string folder)
    {
      //string filename = $"D:\\TESTS\\Leaflet\\TrackSender\\PolygonJson\\{osmid}.json";

      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = $"TrackSender.{folder}.{osmid}.json";

      string s = string.Empty;
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      using (StreamReader reader = new StreamReader(stream))
      {
        s = await reader.ReadToEndAsync();
      }

      // Deserialize the updated product from the response body.
      //var s = await File.ReadAllTextAsync(filename);

      try
      {
        var json = JsonSerializer.Deserialize<Root>(s);
        return json;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return null;
    }
  }
}
