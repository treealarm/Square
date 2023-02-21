using Domain.OptionsModels;
using Domain.Rights;
using Domain.ServiceInterfaces;
using Itinero;
using LeafletAlarms.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
  
  public class RightsController: ControllerBase
  {
    private static Oath2TokenModel _token;
    private static DateTime _tokenExpiration = new DateTime();
    private static DateTime _refreshTokenExpiration = new DateTime();
    private readonly IRightService _rightsService;
    private readonly IOptions<KeycloakSettings> _keyCloakSettings;
    public RightsController(
      IRightService rightsService,
      IOptions<KeycloakSettings> keyCloakSettings
    )
    {
      _rightsService = rightsService;
      _keyCloakSettings = keyCloakSettings;
    }

    [HttpPost]
    [Route("UpdateRights")]
    public async Task<ActionResult<List<ObjectRightsDTO>>> Update(List<ObjectRightsDTO> newObjs)
    {
      await _rightsService.UpdateListAsync(newObjs);
      return CreatedAtAction(nameof(Update), newObjs);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
      try
      {
        await _rightsService.DeleteAsync(id);
      }
      catch (Exception ex)
      {
        return StatusCode(
          StatusCodes.Status500InternalServerError,
          ex.Message
        );
      }

      var listIds = new List<string>() { id };

      var ret = CreatedAtAction(nameof(Delete), null, id);
      return ret;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<ObjectRightsDTO>> Get(string id)
    {
      var obj = await _rightsService.GetListByIdsAsync(new List<string>() { id });

      if (obj is null || obj.Count == 0)
      {
        return NotFound();
      }

      return obj.Values.FirstOrDefault();
    }

    [HttpPost()]
    [Route("GetRights")]
    public async Task<List<ObjectRightsDTO>> GetRights(List<string> ids)
    {
      var data = await _rightsService.GetListByIdsAsync(ids);
      return data.Values.ToList();
    }

    
    async Task GetOath2Token()
    {
      if (_tokenExpiration > DateTime.Now && _token != null)
      {
        return;
      }

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
          { "client_id", "admin-cli" },
          { "username", "admin" },
          { "password", "admin" }
      };

      if (_token != null &&
        !string.IsNullOrEmpty(_token.refresh_token) &&
        _tokenExpiration > DateTime.Now
      )
      {
        dic = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", _token.refresh_token },
            { "client_id", "admin-cli" },
            { "username", "admin" },
            { "password", "admin" }
        };
      }

      _token = null;
      request.Content = new FormUrlEncodedContent(dic);
      

      HttpResponseMessage resp = await client.SendAsync(request);
      var str = await resp.Content.ReadAsStringAsync();

      _token = JsonSerializer.Deserialize<Oath2TokenModel>(str);

      _refreshTokenExpiration = DateTime.Now.AddSeconds(_token.refresh_expires_in);
      _tokenExpiration = DateTime.Now.AddSeconds(_token.expires_in);
    }

    [HttpGet()]
    [Route("GetRoles")]
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
        return modelRoles.Select(m => m.name).ToList();
      }

      return null;
    }

    [HttpGet()]
    [Route("GetRightValues")]
    public Dictionary<string, int> GetRightValues()
    {
      var ret = new Dictionary<string, int>();

      foreach (var right in Enum.GetValues<ObjectRightValueDTO.ERightValue>())
      {
        ret[right.ToString()] = (int)right;
      }
      return ret;
    }
  }
}
