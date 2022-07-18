using Domain;
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
      RunAsync().GetAwaiter().GetResult();
    }

    static async Task<FiguresDTO> UpdateFiguresAsync(FiguresDTO figure)
    {
      HttpResponseMessage response = 
        await client.PostAsJsonAsync(
          $"api/Map/AddTracks", figure);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
      return figure;
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

    static async Task RunAsync()
    {
      // Update port # in the following line.
      client.BaseAddress = new Uri("https://localhost:44307/");
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));

      var figures = await GetByIds(new List<string>() { "62b85c84736cb579f3154917" });

      var circle = figures.circles.FirstOrDefault();
      circle.geometry = new double[] {51.512677840979485, -0.14968839124598346};
      var stat = circle.geometry[0];

      for (double x = 0; x < 0.1; x+=0.001)
      {
        circle.geometry[0] = stat + x;
        await UpdateFiguresAsync(figures);
        await Task.Delay(1000);
      }      
    }
  }
}
