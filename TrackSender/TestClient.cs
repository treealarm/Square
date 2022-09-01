using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TrackSender
{
  internal class TestClient
  {
    static HttpClient client = new HttpClient();

    static TestClient()
    {
      // Update port # in the following line.
      client.BaseAddress = new Uri("https://localhost:44307/");
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));
    }
    public static async Task<FiguresDTO> UpdateFiguresAsync(FiguresDTO figure, string action)
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
      catch (Exception ex)
      {

      }

      return figure;
    }

    public static async Task<List<BaseMarkerDTO>> GetByName(string name)
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

    public static async Task<FiguresDTO> GetByParams(string paramName, string paramValue)
    {
      try
      {
        ObjPropsSearchDTO propFilter = new ObjPropsSearchDTO();
        propFilter.props = new List<KeyValueDTO>();
        propFilter.props.Add(new KeyValueDTO()
        {
          prop_name = paramName,
          str_val = paramValue
        });

        HttpResponseMessage response =
          await client.PostAsJsonAsync(
            $"api/Map/GetByParams", propFilter);

        response.EnsureSuccessStatusCode();

        // Deserialize the updated product from the response body.
        var s = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<FiguresDTO>(s);
        return json;
      }
      catch(Exception ex)
      {

      }
      return null;
    }

    public static async Task<FiguresDTO> GetByIds(List<string> ids)
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

    public static async Task<string> Empty(string ids)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Tracks/Empty", ids);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      return s;
    }
  }
}
