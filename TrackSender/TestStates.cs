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
    FiguresDTO m_figures = new FiguresDTO();
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
      foreach (var osmid in MoscowOsm.osmids)
      {
        string filename = $"D:\\TESTS\\Leaflet\\TrackSender\\PolygonJson\\{osmid[0]}.json";
        if (File.Exists(filename))
        {
          continue;
        }

        var s = await GetByOsmId(osmid[0]);

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
    public async Task<Root> GetMoscowDistrictFromDisk(int osmid)
    {
      string filename = $"D:\\TESTS\\Leaflet\\TrackSender\\PolygonJson\\{osmid}.json";

      // Deserialize the updated product from the response body.
      var s = await File.ReadAllTextAsync(filename);

      try
      {        
        var json = JsonSerializer.Deserialize<Root>(s);
        return json;
      }
      catch (Exception ex)
      {

      }

      return null;
    }

    private async Task<FiguresDTO> CreateOrGetDistrict(int osmid)
    {
      FiguresDTO figures = await _testClient.GetByParams("osmid", osmid.ToString());
      
      if (figures != null && !figures.IsEmpty())
      {
        return figures;        
      }

      figures = new FiguresDTO();
      figures.circles = new List<FigureCircleDTO>();
      figures.polygons = new List<FigurePolygonDTO>();

      var root = await GetMoscowDistrictFromDisk(osmid);

      if (root == null)
      {
        return null;
      }

      var me = MoscowOsm.osmids.Where(o => o[0] == osmid).FirstOrDefault();
      var parent = me[1];

      var parentPolygon = m_figures.polygons
        .Where(p => p.extra_props
          .Any(e => e.prop_name == "osmid" && e.str_val == parent.ToString())   
        ).FirstOrDefault();

      var geoObj = root;
      {
        if (geoObj.geometry.type == "MultiPolygon" ||
          geoObj.geometry.type == "Polygon")
        {
          MultiPolygon coords;

          if (geoObj.geometry.type == "Polygon")
          {
            var polygon =
              JsonSerializer.Deserialize<Polygon>(geoObj.geometry.coordinates.ToString());
            coords = new MultiPolygon()
            {
              polygon
            };
          }
          else
          {
            coords = 
              JsonSerializer.Deserialize<MultiPolygon>(geoObj.geometry.coordinates.ToString());
          }
            

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

            figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "osmid",
                str_val = osmid.ToString()
              }
            };

            if (parentPolygon != null)
            {
              figure.parent_id = parentPolygon.parent_id;

              if (parent == MoscowOsm.osmids.First()[0])
              {
                figure.zoom_level = "9";
              }
            }
            else
            {
              figure.zoom_level = "8";
            }

            figures.polygons.Add(figure);
          }

        }
        else
        if (geoObj.geometry.type == "Point")
        {
          var coords =
            JsonSerializer.Deserialize<Geo2DCoordDTO>(geoObj.geometry.coordinates.ToString());
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

          figure.extra_props = new List<ObjExtraPropertyDTO>()
            {
              new ObjExtraPropertyDTO()
              {
                prop_name = "osmid",
                str_val = osmid.ToString()
              }
            };

          if (parentPolygon != null)
          {
            figure.parent_id = parentPolygon.parent_id;

            if (parent == MoscowOsm.osmids.First()[0])
            {
              figure.zoom_level = "8";
            }
          }
          else
          {
            figure.zoom_level = "8";
          }

          figures.circles.Add(figure);
        }
        else
        {
          // Undefined.
        }
      }

      return figures;
    }
    public async Task BuildMoscow()
    {      
      foreach (var osmid in MoscowOsm.osmids)
      {
        var figure = await CreateOrGetDistrict(osmid[0]);

        if (figure != null && !figure.IsEmpty())
        {
          m_figures?.polylines.AddRange(figure?.polylines);
          m_figures?.circles.AddRange(figure?.circles);
          m_figures?.polygons.AddRange(figure?.polygons);
        }
        else
        {
          figure = await CreateOrGetDistrict(osmid[0]);
          // Empty figure?
        }
      }

      var figuresCreated = await _testClient.UpdateFiguresAsync(m_figures, "AddTracks");
    }
  }
}
