using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using System;
using System.Collections.Generic;
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
      await BuildMoscow();
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

            foreach(var c in coord[0])
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
