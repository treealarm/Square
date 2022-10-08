using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using TrackSender.Models;

namespace TrackSender
{
  internal class TestStates
  {
    //https://wiki.openstreetmap.org/wiki/RU:%D0%9C%D0%BE%D1%81%D0%BA%D0%B2%D0%B0/%D0%90%D0%B4%D0%BC%D0%B8%D0%BD%D0%B8%D1%81%D1%82%D1%80%D0%B0%D1%82%D0%B8%D0%B2%D0%BD%D0%BE-%D1%82%D0%B5%D1%80%D1%80%D0%B8%D1%82%D0%BE%D1%80%D0%B8%D0%B0%D0%BB%D1%8C%D0%BD%D0%BE%D0%B5_%D0%B4%D0%B5%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5
    //childeren of moscow:
    //https://nominatim.openstreetmap.org/details?osmtype=R&osmid=102269&addressdetails=1&hierarchy=1&group_hierarchy=1&format=json&pretty=1
    //https://nominatim.openstreetmap.org/details.php?osmtype=R&osmid=102269&addressdetails=1&hierarchy=0&group_hierarchy=1&format=json
    //https://nominatim.openstreetmap.org/details?place_id=337939658&format=json&pretty=1&hierarchy=1
    //https://nominatim.openstreetmap.org/details?place_id=337939658&format=json&pretty=1
    string main_city = @"https://nominatim.openstreetmap.org/search.php?city=moscow&country=russia&polygon_geojson=1&format=json";
    TestClient _testClient = new TestClient();
    FiguresDTO _figures = new FiguresDTO();
    HttpClient _client = new HttpClient();
    public async Task RunAsync()
    {
      _client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
      _client.DefaultRequestHeaders.Accept.Clear();
      _client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));

      _client.DefaultRequestHeaders.UserAgent.Clear();
      _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("f1ana.Nominatim.API", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
      //await SaveMoscowToLocalDisk();
      await BuildMoscow();
    }

    private async Task SaveMoscowToLocalDisk()
    {
      List<int> osmids = new List<int>()
      {
      102269,
      2162196,
      1257484,
      2162195,
      1255942,
      1257218,
      364001,
      1275551,
      1275608,
      1257786,
      1255987,
      1275627,
      162903,
      446079,
      445298,
      446082,
      446078,
      445285,
      445297,
      445278,
      445275,
      445277,
      445273,
      445283,
      445281,
      445280,
      445276,
      445274,
      445299,
      1252558,
      1250526,
      446083,
      1252465,
      446085,
      1252424,
      446084,
      1252448,
      1250724,
      1252407,
      1250619,
      445282,
      446272,
      1250618,
      446086,
      445284,
      446087,
      446271,
      1320234,
      1319060,
      574667,
      1255704,
      1255775,
      1319142,
      1319263,
      1319245,
      1255576,
      1319078,
      1298976,
      1255602,
      1299013,
      1299106,
      1255680,
      1299031,
      364551,
      1278703,
      548619,
      455208,
      444812,
      1278064,
      455203,
      455222,
      1255577,
      1278046,
      455451,
      1255563,
      455184,
      1278096,
      1282181,
      950639,
      950658,
      531287,
      1281209,
      444908,
      531264,
      455528,
      535655,
      455460,
      535662,
      455539,
      456807,
      950641,
      535680,
      951305,
      950664,
      1304596,
      1281220,
      1281648,
      1292286,
      952191,
      1292211,
      1281702,
      1292499,
      1257455,
      951336,
      1281263,
      1257403,
      951334,
      226149,
      446114,
      181288,
      240229,
      226927,
      1257472,
      446117,
      1292749,
      1292679,
      446115,
      446116,
      1292731,
      446112,
      446111,
      446092,
      442741,
      442733,
      446081,
      445279,
      434560,
      428431,
      431464,
      446080,
      1320358,
      1320371,
      1320566,
      1320570,
      1320510,
      1320424,
       };

      foreach (var osmid in osmids)
      {
        string filename = $"D:\\TESTS\\Leaflet\\TrackSender\\PolygonJson\\{osmid}.json";
        if (File.Exists(filename))
        {
          continue;
        }

        var s = await GetByOsmId(osmid);

        if (s == null)
        {
          continue;
        }
        await File.WriteAllTextAsync(
          filename, s);
      }
    }
    public async Task<string> GetByOsmId(int osmid)
    {
      try
      {
        var request = $"details?osmtype=R&osmid={osmid}&polygon_geojson=1&format=json&pretty=1";
        HttpResponseMessage response =
          await _client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        var s = await response.Content.ReadAsStringAsync();

        return s;
      }
      catch(Exception ex)
      {

      }
      return null;
      
    }
    public async Task<List<Root>> GetMoscow()
    {
      HttpResponseMessage response =
        await _client.GetAsync(
          $"search?city=moscow&country=russia&polygon_geojson=1&format=json");

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      try
      {
        var json = JsonSerializer.Deserialize<List<Root>>(s);
        return json;
      }
      catch (Exception ex)
      {

      }

      return null;
    }

    public async Task BuildMoscow()
    {
      FiguresDTO figures = new FiguresDTO();
      figures.circles = new List<FigureCircleDTO>();
      figures.polygons = new List<Domain.GeoDTO.FigurePolygonDTO>();

      var root = await GetMoscow();

      if (root == null || root.Count == 0)
      {
        return;
      }

      foreach (var geoObj in root)
      {
        if (geoObj.geojson.type == "MultiPolygon")
        {
          MultiPolygon coords =
            JsonSerializer.Deserialize<MultiPolygon>(geoObj.geojson.coordinates.ToString());

          foreach (var coord in coords)
          {
            var start =
              new GeometryPolygonDTO();

            foreach (var c in coord[0])
            {
              //Suffle this shit.
              var temp = c.X;
              c.X = c.Y;
              c.Y = temp;
            }

            start.coord = coord[0];

            var figure = new FigurePolygonDTO()
            {
              name = geoObj.osm_id.ToString(),
              zoom_level = "13",
              geometry = start
            };
            figures.polygons.Add(figure);
          }

        }

        if (geoObj.geojson.type == "Point")
        {
          var coords =
            JsonSerializer.Deserialize<Geo2DCoordDTO>(geoObj.geojson.coordinates.ToString());
          var start =
              new GeometryCircleDTO(
                new Geo2DCoordDTO() {
                  coords.X,
                  coords.Y }
                );

          var figure = new FigureCircleDTO()
          {
            name = geoObj.osm_id.ToString(),
            radius = 222,
            zoom_level = "13",
            geometry = start
          };
          figures.circles.Add(figure);
        }
      }

      var figuresCreated = await _testClient.UpdateFiguresAsync(figures, "AddTracks");
    }
  }
}
