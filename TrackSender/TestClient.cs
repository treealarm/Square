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
using TrackSender.Authentication;

namespace TrackSender
{
  internal class TestClient
  {
    HttpClient _client = new HttpClient();

    public TestClient(HttpClient client)
    {
      _client = client;
    }
    public async Task<FiguresDTO> UpdateTracksAsync(FiguresDTO figure, string action)
    {
      try
      {
        HttpResponseMessage response =
        await _client.PostAsJsonAsync(
          $"api/Tracks/{action}", figure);

      response.EnsureSuccessStatusCode();

      // Deserialize the updated product from the response body.
      var s = await response.Content.ReadAsStringAsync();

      
        if (action == "AddTracks")
        {
          FiguresDTO json = JsonSerializer.Deserialize<FiguresDTO>(s);
          return json;
        }
        else
        {
          Dictionary<string, TimeSpan> json = JsonSerializer.Deserialize<Dictionary<string, TimeSpan>>(s);
          Console.WriteLine($"---------------------------------->");
          foreach (var pair in json)
          {
            Console.WriteLine($"{pair.Key}-> {(int)pair.Value.TotalMilliseconds} [ms]");
          }
          Console.WriteLine($"<----------------------------------");
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
        await _client.PostAsJsonAsync($"api/map/UpdateFigures", figure);

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
        await _client.PostAsJsonAsync($"api/map/UpdateBase", updatedMarker);

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
      try
      {
        HttpResponseMessage response =
        await _client.PostAsJsonAsync(
          $"api/Map/GetByName", name);

        response.EnsureSuccessStatusCode();

        // Deserialize the updated product from the response body.
        var s = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<List<BaseMarkerDTO>>(s);
        return json;
      }
      catch(Exception ex)
      {
        Console.WriteLine($"{ex.Message}");
        return null;
      }
    }

    public async Task<FiguresDTO> GetByParams(
      string paramName,
      string paramValue,
      string start_id,
      int count)
    {
      try
      {
        SearchFilterDTO filter = new SearchFilterDTO();
        filter.count = count;
        filter.start_id = start_id;
        filter.forward = true;
        filter.property_filter = new ObjPropsSearchDTO();
        filter.property_filter.props = new List<KeyValueDTO>();
        filter.property_filter.props.Add(new KeyValueDTO()
        {
          prop_name = paramName,
          str_val = paramValue
        });

        HttpResponseMessage response =
          await _client.PostAsJsonAsync(
            $"api/Map/GetByParams", filter);

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
        await _client.PostAsJsonAsync(
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
        await _client.PostAsJsonAsync(
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
                await _client.PostAsJsonAsync(
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
