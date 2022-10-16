using Domain;
using Domain.States;
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
    HttpClient client = new HttpClient();

    public TestClient()
    {
      // Update port # in the following line.
      
      client.BaseAddress = new Uri(App.Default.ServerAddress);
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));
    }
    public async Task<FiguresDTO> UpdateTracksAsync(FiguresDTO figure, string action)
    {
      var str = JsonSerializer.Serialize(figure);

      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Tracks/{action}", figure);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      try
      {
        if (action == "AddTracks")
        {
          FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
          return json;
        }
        else
        {
          Dictionary<string, TimeSpan> json = JsonSerializer.Deserialize<Dictionary<string, TimeSpan>>(s);
          Console.WriteLine($"UpdateTrack:");
          foreach (var pair in json)
          {
            Console.WriteLine($"{pair.Key}-> {(int)pair.Value.TotalMilliseconds} [ms]");
          }
          Console.WriteLine($"-------");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return figure;
    }

    public async Task<FiguresDTO> UpdateFiguresAsync(FiguresDTO figure)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync($"api/map/UpdateFigures", figure);

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
        Console.WriteLine(ex.Message);
      }

      return figure;
    }

    public async Task<BaseMarkerDTO> UpdateBase(BaseMarkerDTO updatedMarker)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync($"api/map/UpdateBase", updatedMarker);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      try
      {
        BaseMarkerDTO json = JsonSerializer.Deserialize<BaseMarkerDTO>(s);
        return json;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return null;
    }

    public async Task<List<BaseMarkerDTO>> GetByName(string name)
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

    public async Task<FiguresDTO> GetByParams(string paramName, string paramValue)
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
        Console.WriteLine(ex.Message);
      }
      return null;
    }

    public async Task<FiguresDTO> GetByIds(List<string> ids)
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

    public async Task<string> Empty(string ids)
    {
      HttpResponseMessage response =
        await client.PostAsJsonAsync(
          $"api/Tracks/Empty", ids);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      return s;
    }

    public async Task UpdateStates(List<ObjectStateDTO> newObjs)
    {
      try
      {
        HttpResponseMessage response =
                await client.PostAsJsonAsync(
                  $"api/States/UpdateStates", newObjs);

        response.EnsureSuccessStatusCode();

        // Deserialize the updated product from the response body.
        var s = await response.Content.ReadAsStringAsync();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
      }      
    }
  }
}
