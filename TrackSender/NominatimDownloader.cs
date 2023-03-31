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
using System.Runtime.CompilerServices;

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
   
    public async Task RunAsync(
      CancellationToken token,
      List<Task> tasks,
      [CallerFilePath] string callerFilePath = null
    )
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

      var callerDir = Path.GetDirectoryName(callerFilePath);

      //await SaveMoscowToLocalDisk();

      //await SaveJsonToLocalDisk("MKAD");

      //await SaveJsonToLocalDisk("CKAD", ckadFolder);

      //var folder = Path.Combine(callerDir, "SAD");
     // await SaveJsonToLocalDisk("SAD", folder);

      //var mkad = await GetMkadPolyline();
    }

    static async Task<List<int>> GetOsmIdsFromXml(string resourceFolder)
    {
      var assembly = Assembly.GetExecutingAssembly();

      var resourceName = $"TrackSender.{resourceFolder}.{resourceFolder}.xml";

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
    private async Task SaveJsonToLocalDisk(string xmlResource, string folder)
    {
      string s;
      var osmIds = await GetOsmIdsFromXml(xmlResource);

      foreach (var osmid in osmIds)
      {
        string filename = $"{osmid}.json";
        filename = Path.Combine(folder, filename);

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

    public static async Task<List<FigureGeoDTO>> GetRoadPolyline(string resourceFolder)
    {
      var list1 = new List<Geo2DCoordDTO>();

      var osmIds = await GetOsmIdsFromXml(resourceFolder);

      foreach (var osmid in osmIds)
      {
        var geoObj = await NominatimProcessor.GetOsmFigureFromDisk(osmid, resourceFolder);

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
            list1.AddRange(coords);
          }
          else
          {
            throw new Exception($"Bad {resourceFolder} json {osmid}");
            // Undefined.
          }
        }
      }
     
      var startPoint = list1.FirstOrDefault();
      list1.RemoveAt(0);

      var commonList = new List<List<Geo2DCoordDTO>>();
      var list2 = new List<Geo2DCoordDTO>();
      commonList.Add(list2);

      list2.Add(startPoint);

      while (list1.Count > 0)
      {
        var fig = list2.FirstOrDefault();
        double curMaxLeft = double.MaxValue;
        var figNearestLeft = FindNearest(fig, list1, out curMaxLeft);

        fig = list2.LastOrDefault();
        double curMaxRight = double.MaxValue;
        var figNearestRight = FindNearest(fig, list1, out curMaxRight);

        if (curMaxLeft > 0.1 && curMaxRight > 0.1)
        {
          list2 = new List<Geo2DCoordDTO>();
          commonList.Add(list2);
        }

        if (curMaxRight < curMaxLeft)
        {
          //if (curMaxRight < 0.1)
          {
            list2.Add(figNearestRight);
          }
          
          list1.Remove(figNearestRight);
        }
        else
        {
          //if (curMaxLeft < 0.1)
          {
            list2.Insert(0, figNearestLeft);
          }
          
          list1.Remove(figNearestLeft);
        }        
      }

      ////


      var longList = commonList.MaxBy(l => l.Count);
      commonList.Remove(longList);

      

      while (commonList.Count > 0)
      {
        double curDist = double.MaxValue;
        double curMax = double.MaxValue;
        bool bFirst = true;

        List<Geo2DCoordDTO> lListToInsert = null;
        Geo2DCoordDTO lPosToInsert = null;

        foreach (var listToAnalize in commonList)
        {
          var first = listToAnalize.First();
          var figNearest = FindNearest(first, longList, out curDist);

          if (curMax > curDist)
          {
            bFirst = true;
            curMax = curDist;
            lListToInsert = listToAnalize;
            lPosToInsert = figNearest;
          }

          var last = listToAnalize.Last();
          figNearest = FindNearest(last, longList, out curDist);

          if (curMax > curDist)
          {
            bFirst = false;
            curMax = curDist;
            lListToInsert = listToAnalize;
            lPosToInsert = figNearest;
          }
        }

        if (bFirst)
        {
          longList.InsertRange(longList.IndexOf(lPosToInsert) + 1, lListToInsert);
        }
        else
        {
          longList.InsertRange(longList.IndexOf(lPosToInsert) - 1, lListToInsert);
        }

        commonList.Remove(lListToInsert);
      }


      var result = new List<FigureGeoDTO>();

      var figure = new FigureGeoDTO()
      {
        name = resourceFolder,
        //zoom_level = "13",
        geometry = new GeometryPolylineDTO()
        {
          coord = longList
          //new List<Geo2DCoordDTO>()
        }
      };

      figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = resourceFolder,
                str_val = "true"
              }
            };
      result.Add(figure);
      return result;
    }

    static Geo2DCoordDTO FindNearest(
      Geo2DCoordDTO fig,
      List<Geo2DCoordDTO> list,
      out double curMax)
    {
      Geo2DCoordDTO retVal = null;
      Geo2DCoordDTO geo1 = null;


      geo1 = fig;

      curMax = double.MaxValue;

      foreach (var f in list)
      {
        Geo2DCoordDTO geo2 = f;
          
        var dx = Math.Abs(geo1.X - geo2.X);
        var dy = Math.Abs(geo1.Y - geo2.Y);

        var curD = Math.Sqrt(dx * dx + dy * dy);

        if (curD < curMax)
        {
          curMax = curD;
          retVal = f;
        }
      }

      return retVal;
    }

    static public async Task<string> GetResource(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();

      string s = string.Empty;
      using (Stream stream = assembly.GetManifestResourceStream(resourceName))
      using (StreamReader reader = new StreamReader(stream))
      {
        s = await reader.ReadToEndAsync();
      }
      return s;
    }
    public static async Task<Root> GetOsmFigureFromDisk(int osmid, string folder)
    {
      //string filename = $"D:\\TESTS\\Leaflet\\TrackSender\\PolygonJson\\{osmid}.json";

      var resourceName = $"TrackSender.{folder}.{osmid}.json";

      string s = string.Empty;
      s = await GetResource(resourceName);

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
