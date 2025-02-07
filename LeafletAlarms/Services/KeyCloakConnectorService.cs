using Domain;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using LeafletAlarms.Authentication.Models;

namespace LeafletAlarms.Authentication
{
    //http://localhost:8080/realms/myrealm/
  public class KeyCloakConnectorService
  {
    private Oath2TokenModel _token;
    private DateTime _tokenExpiration = new DateTime();
    private DateTime _refreshTokenExpiration = new DateTime();
    private readonly IOptions<KeycloakSettings> _keyCloakSettings;
    public string GetRealmName()
    {
      return _keyCloakSettings.Value.RealmName;
    }

    public string GetBaseAddr()
    {
      return _keyCloakSettings.Value.BaseAddr;
    }

    public KeyCloakConnectorService(IOptions<KeycloakSettings> keyCloakSettings)
    {
      _keyCloakSettings = keyCloakSettings;
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
          new Uri(_keyCloakSettings.Value.BaseAddr, UriKind.Absolute),
          new Uri($"realms/master/protocol/openid-connect/token", UriKind.Relative),
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
          { "client_id", _keyCloakSettings.Value.admin_client_id },
          { "username", _keyCloakSettings.Value.admin_name },
          { "password", _keyCloakSettings.Value.admin_password }
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

        _token = JsonSerializer.Deserialize<Oath2TokenModel>(str);

        _refreshTokenExpiration = DateTime.Now.AddSeconds(_token.refresh_expires_in);
        _tokenExpiration = DateTime.Now.AddSeconds(_token.expires_in);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    public async Task<List<string>> GetRoles()
    {
      await GetOath2Token();

      if (Uri.TryCreate(
        new Uri(_keyCloakSettings.Value.BaseAddr, UriKind.Absolute),
        new Uri($"admin/realms/{_keyCloakSettings.Value.RealmName}/roles", UriKind.Relative),
        out var url)
      )
      {
        var request = new HttpRequestMessage(HttpMethod.Get,
          url.ToString()
        );
        var client = new HttpClient();
        request.Headers.Add("authorization", "Bearer " + _token.access_token);
        request.Headers.Add("cache-control", "no-cache");
        HttpResponseMessage resp = await client.SendAsync(request);

        var modelRoles =
          JsonSerializer.Deserialize<List<RoleModel>>(
            await resp.Content.ReadAsStringAsync()
          );
        return modelRoles.Select(m => m.name)
          .Where(m => m != RoleConstants.admin)
          .ToList();
      }

      return null;
    }

    public async Task<RealmInfoModel> GetRealmInfo()
    {
      await GetOath2Token();

      if (_token == null)
      {
        return null;
      }

      if (Uri.TryCreate(
        new Uri(_keyCloakSettings.Value.BaseAddr, UriKind.Absolute),
        new Uri($"realms/{_keyCloakSettings.Value.RealmName}", UriKind.Relative),
        out var url)
      )
      {
        try
        {
          var request = new HttpRequestMessage(HttpMethod.Get,
            url.ToString()
          );
          var client = new HttpClient();
          request.Headers.Add("authorization", "Bearer " + _token.access_token);
          request.Headers.Add("cache-control", "no-cache");
          HttpResponseMessage resp = await client.SendAsync(request);

          var realmInfo =
            JsonSerializer.Deserialize<RealmInfoModel>(
              await resp.Content.ReadAsStringAsync()
            );
          return realmInfo;
        }
        catch( Exception ex )
        { 
          Console.WriteLine( ex.ToString() );
        }
      }
      return null;
    }
    
  }
}
