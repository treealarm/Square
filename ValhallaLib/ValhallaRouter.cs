using LeafletAlarmsGrpc;
using System.Web;
using System;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Text;

namespace ValhallaLib
{
  public class ValhallaRouter
  {
    private string _base_url;
    private HttpClient _client = new HttpClient();
    public ValhallaRouter(string url)
    {
      _base_url = url;
      _client.BaseAddress = new Uri(_base_url);
      _client.DefaultRequestHeaders.Accept.Clear();
      _client.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task<RootResponse?> GetRoutFromValhalla(string request)
    {
      try
      {
        HttpResponseMessage response = await _client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        var s = await response.Content.ReadAsStringAsync();
        RootResponse? myDeserializedClass = JsonSerializer.Deserialize<RootResponse>(s);
        return myDeserializedClass;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"{ex.Message}");        
      }
      return null;
    }

    // Handy lambda to turn a few bytes of an encoded string into an integer
    private int deserialize(ref string encoded, ref int i, int previous)
    {
      // Grab each 5 bits and mask it in where it belongs using the shift
      int byteData, shift = 0, result = 0;
      do {
          byteData = (int)(encoded[i++]) - 63;
          result |= (byteData & 0x1f) << shift;
          shift += 5;
      } while (byteData >= 0x20);
        // Undo the left shift from above or the bit flipping and add to previous
        // since its an offset
        int v = result & 1;
        return previous + (v == 0 ? ~(result >> 1) : (result >> 1));
    }
    private List<ProtoCoord> decode(string? encoded) 
    {
      if (encoded == null)
      {
        return new List<ProtoCoord>();
      }
      const double kPolylinePrecision = 1E6;
      const double kInvPolylinePrecision = 1.0 / kPolylinePrecision;

      int i = 0;     // what byte are we looking at



    // Iterate over all characters in the encoded string
    List<ProtoCoord> shape = new List<ProtoCoord>();
    int last_lon = 0, last_lat = 0;

    while (i < encoded.Length)
    {
      // Decode the coordinates, lat first for some reason
      int lat = deserialize(ref encoded, ref i, last_lat);
      int lon = deserialize(ref encoded, ref i, last_lon);

      // Shift the decimal point 5 places to the left
      shape.Add(new ProtoCoord() 
      {
        Lat = Convert.ToDouble(lat)  * kInvPolylinePrecision , 
        Lon  = Convert.ToDouble(lon) * kInvPolylinePrecision 
      });

      // Remember the last one we encountered
      last_lon = lon;
      last_lat = lat;
    }
    return shape;
  }

    public async Task<List<ProtoCoord>> GetRoute(ProtoCoord start, ProtoCoord end)
    {
      var routes = new List<ProtoCoord>();
      UriBuilder builder = new UriBuilder(_base_url);
      builder.Path = "optimized_route";
      
      var rootJson = new Root();
      rootJson.locations.Add(new LocationRequest() { lat = start.Lat, lon = start.Lon });
      rootJson.locations.Add(new LocationRequest() { lat = end.Lat, lon = end.Lon });
      rootJson.id = "valhalla";
      
      var query = HttpUtility.ParseQueryString("");
      query["json"] = JsonSerializer.Serialize(rootJson);

      builder.Query = query.ToString();

      var result = await GetRoutFromValhalla(builder.ToString());
      var legs = result?.trip?.legs;

      legs?.ForEach(leg =>
      {
        routes.AddRange(decode(leg?.shape));
      });
       
      return routes;
    }
  }
}