using Domain;
using KeycloakAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;

namespace mt_admin
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IKeycloakAdminClient _kcAdmin;

    public AuthController(IKeycloakAdminClient kcAdmin)
    {
      _kcAdmin = kcAdmin;
    }

    // Right after the dev stack starts, Kestrel is already accepting requests before
    // InitHostedService's Keycloak warm-up (see ConfigureJwtBearerOptions.EnsureKeyLoadedAsync)
    // finishes — Keycloak's container can report healthy before its realm import completes.
    // The very first login can race that window and hit a connection-level failure even
    // though the credentials are fine. Retry only on transient connectivity errors, not on
    // Keycloak's own rejection of bad credentials (4xx).
    private static bool IsTransientKeycloakError(Exception ex) =>
      ex is SocketException ||
      ex is TaskCanceledException ||
      (ex is HttpRequestException hre && (hre.StatusCode == null || (int)hre.StatusCode >= 500));

    [HttpPost("customer_login")]
    [AllowAnonymous]
    public async Task<IActionResult> CustomerLogin(CustomerLoginDto dto)
    {
      var dbRealmName = EnvConfig.Require("DB_REALM_NAME");
      var client_id = Constants.PubClient;

      const int maxAttempts = 5;
      for (var attempt = 1; attempt <= maxAttempts; attempt++)
      {
        try
        {
          var content = await _kcAdmin.GetTokenAsync(dbRealmName, client_id, dto.Username, dto.Password);
          return Ok(content);
        }
        catch (Exception ex) when (attempt < maxAttempts && IsTransientKeycloakError(ex))
        {
          await Task.Delay(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex)
        {
          return StatusCode(500, ex.Message);
        }
      }

      return StatusCode(500, "Keycloak unavailable");
    }
    [HttpGet("ValidateToken")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken([FromHeader(Name = "Authorization")] string authHeader)
    {
      await Task.Delay(0);
      if (string.IsNullOrEmpty(authHeader)) 
        return Unauthorized();

      var token = authHeader.Replace("Bearer ", "");
      var valid = _kcAdmin.IsTokenValid(token); // метод проверки токена через Keycloak или внутреннюю логику
      if (!valid) 
        return Unauthorized();

      return Ok();
    }

    [HttpPost("RefreshToken")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest req)
    {
      await Task.Delay(0);

      if (string.IsNullOrEmpty(req.refresh_token))
        return Unauthorized("No refresh token");

      var dbRealmName = EnvConfig.Require("DB_REALM_NAME");


      // Проверка refresh токена
      var newAccessToken = await _kcAdmin.RefreshAccessToken(
        dbRealmName, 
        Constants.PubClient, 
        req.refresh_token);

      if (newAccessToken == null)
        return Unauthorized("Refresh token invalid");

      return Ok(newAccessToken);
    }

  }
}
