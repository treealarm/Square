using Domain.OptionsModels;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TrackSender.Authentication
{
  public class Oath2TokenModel
  {
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public int refresh_expires_in { get; set; }
    public string refresh_token { get; set; }
    public string token_type { get; set; }

    [JsonPropertyName("not-before-policy")]
    public int notbeforepolicy { get; set; }
    public string session_state { get; set; }
    public string scope { get; set; }
  }

  //http://localhost:8080/realms/myrealm/
  public class KeyCloakConnectorService
  {
    private Oath2TokenModel _token;
    private DateTime _tokenExpiration = new DateTime();
    private DateTime _refreshTokenExpiration = new DateTime();

    public string GetToken()
    {
      return _token.access_token;
    }
    public string GetRealmName()
    {
      return "myrealm";
    }

    public string GetBaseAddr()
    {
      return "http://keycloakservice:8080";
    }

    public KeyCloakConnectorService()
    {
    }
    public async Task GetOath2Token()
    {
      if (_tokenExpiration > DateTime.Now && _token != null)
      {
        return;
      }

      try
      {
        var client = new HttpClient();

        Uri url;

        if (!Uri.TryCreate(
          new Uri(GetBaseAddr(), UriKind.Absolute),
          new Uri($"realms/{GetRealmName()}/protocol/openid-connect/token", UriKind.Relative),
          out url)
        )
        {
          return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post,
          url.ToString()
        );

        request.Headers.Add("cache-control", "no-cache");
        request.Headers.Add(
           HttpRequestHeader.ContentType.ToString(), //useless
           "application/x-www-form-urlencoded"
        );

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var dic = new Dictionary<string, string>
      {
          { "grant_type", "password" },
          { "client_id", "pubclient" },
          { "username", "myuser" },
          { "password", "myuser" }
      };

        if (_token != null &&
          !string.IsNullOrEmpty(_token.refresh_token) &&
          _refreshTokenExpiration > DateTime.Now
        )
        {
          dic["grant_type"] = "refresh_token";
          dic["refresh_token"] = _token.refresh_token;
        }

        _token = null;
        request.Content = new FormUrlEncodedContent(dic);


        HttpResponseMessage resp = await client.SendAsync(request);
        var str = await resp.Content.ReadAsStringAsync();
        Console.WriteLine( str );

        _token = JsonSerializer.Deserialize<Oath2TokenModel>(str);

        _refreshTokenExpiration = DateTime.Now.AddSeconds(_token.refresh_expires_in);
        _tokenExpiration = DateTime.Now.AddSeconds(_token.expires_in);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }   
  }
}
