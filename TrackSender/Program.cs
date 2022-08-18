using Domain;
using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TrackSender
{
  class Program
  {
    static HttpClient client = new HttpClient();

    static void Main(string[] args)
    {
      try
      {
        RunAsync().GetAwaiter().GetResult();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    static async Task<FiguresDTO> UpdateFiguresAsync(FiguresDTO figure, string action)
    {
      HttpResponseMessage response = 
        await client.PostAsJsonAsync(
          $"api/Tracks/{action}", figure);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      try
      {
        FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
        return json;
      }
      catch(Exception ex)
      {

      }
      
      return figure;
    }

    static async Task<List<BaseMarkerDTO>> GetByName(string name)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Map/GetByName", name);
      
      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      var json = JsonSerializer.Deserialize<List<BaseMarkerDTO>>(s);
      return json;
    }

    static async Task<FiguresDTO> GetByIds(List<string> ids)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Map/GetByIds", ids);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
      return json;
    }

    static async Task<string> Empty(string ids)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Tracks/Empty", ids);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      return s;
    }

    static async Task RunAsync()
    {
      // Update port # in the following line.
      client.BaseAddress = new Uri("https://localhost:44307/");
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));

      var markers = await GetByName("test_track");
      FiguresDTO figures;
      FigureCircleDTO circle;

      if (markers == null || markers.Count == 0)
      {
        figures = new FiguresDTO();
        figures.circles = new List<FigureCircleDTO>();
        circle = new FigureCircleDTO()
        {
          name = "test_track",
          radius = 333,
          zoom_level = "13"
        };
        figures.circles.Add(circle);

        circle.geometry = new GeometryCircleDTO(new Geo2DCoordDTO() { 51.512677840979485, -0.14968839124598346 });
        figures = await UpdateFiguresAsync(figures, "AddTracks");
      }
      else
      {
        figures = await GetByIds(new List<string>() { markers.FirstOrDefault().id });
      }
      
      //var t1 = DateTime.Now;

      //for (int i = 0; i< 100000; i++)
      //{
      //  var sfigures = await Empty("62b85c84736cb579f3154917");

      //  if (i%2000 == 0)
      //  {
      //    var t3 = (DateTime.Now - t1).TotalSeconds;
      //    Console.WriteLine($"{i}-{t3}");
      //    t1 = DateTime.Now;
      //  }
      //}

      //var t2 = (DateTime.Now - t1).TotalSeconds;

      circle = figures.circles.FirstOrDefault();

      if (circle == null)
      {
        return;
      }

      circle.geometry = new GeometryCircleDTO(new Geo2DCoordDTO() { 51.512677840979485, -0.14968839124598346 }) ;
      var stat_y = circle.geometry.coord[0];
      var stat_x = circle.geometry.coord[1];
      Random rand = new Random();

      for (double dy = 0; dy < 0.1; dy+=0.001)
      {
        var dx = rand.NextDouble() / 500;
        circle.geometry.coord[0] = stat_y + dy;
        circle.geometry.coord[1] = stat_x + dx;
        circle.zoom_level = "13";
        Console.WriteLine(JsonSerializer.Serialize(circle.geometry));
        await UpdateFiguresAsync(figures, "UpdateTracks");
        await Task.Delay(1000);
      }      
    }
  }
}
