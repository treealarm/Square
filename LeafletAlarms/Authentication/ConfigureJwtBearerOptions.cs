
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace LeafletAlarms.Authentication
{
  public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
  {
    private readonly KeyCloakConnectorService _myService;
    private volatile RsaSecurityKey _securityKey;

    public ConfigureJwtBearerOptions(KeyCloakConnectorService myService)
    {
      _myService = myService;
    }

    // ВАЖНО: вызывается из IssuerSigningKeyResolver на КАЖДЫЙ запрос с Bearer-токеном.
    // Здесь не должно быть ни I/O, ни блокировок (.Result) — иначе каждый
    // авторизованный запрос синхронно ждёт поход в Keycloak (и ещё и роняет пул потоков).
    // Ключ заранее прогревается через EnsureKeyLoadedAsync (см. InitHostedService).
    private RsaSecurityKey GetKey()
    {
      return _securityKey;
    }

    // Прогрев публичного ключа реалма. Дергается один раз при старте приложения,
    // с ретраями — Keycloak может быть ещё не готов в момент запуска.
    public async Task<bool> EnsureKeyLoadedAsync(int maxAttempts, TimeSpan retryDelay, CancellationToken token)
    {
      if (_securityKey != null)
      {
        return true;
      }

      for (int attempt = 1; attempt <= maxAttempts && !token.IsCancellationRequested; attempt++)
      {
        var realmInfo = await _myService.GetRealmInfo();
        var publicKeyJWT = realmInfo?.public_key;

        if (!string.IsNullOrEmpty(publicKeyJWT))
        {
          _securityKey = ConfigureAuthentificationServiceExtensions.BuildRSAKey(publicKeyJWT);
          return true;
        }

        Console.WriteLine($"get publicKeyJWT: FAILED, attempt {attempt}/{maxAttempts}");

        if (attempt < maxAttempts)
        {
          await Task.Delay(retryDelay, token);
        }
      }

      return false;
    }

    public void Configure(string name, JwtBearerOptions o)
    {
      // check that we are currently configuring the options for the correct scheme
      if (name == JwtBearerDefaults.AuthenticationScheme)
      { 
        var realmName = KeyCloakConnectorService.GetRealmName();

        var BaseAddr = KeyCloakConnectorService.GetBaseAddr();

        //"http://localhost:8080/realms/myrealm"
        Uri validIssuer;
        if (Uri.TryCreate(
          new Uri(BaseAddr, UriKind.Absolute),
          new Uri($"realms/{realmName}", UriKind.Relative),
          out validIssuer)
        )
        {

        }



        o.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateAudience = false,
          ValidateIssuer = false,
          ValidIssuers = new[] { validIssuer.ToString() },
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = GetKey(),
          ValidateLifetime = true          
        };

        o.TokenValidationParameters.IssuerSigningKeyResolver =
          (
            string token,
            SecurityToken securityToken,
            string kid,
            TokenValidationParameters validationParameters
        ) =>
        {
          var key = GetKey();

          return key != null
            ? new List<SecurityKey>() { key }
            : Array.Empty<SecurityKey>();
        };

        o.Events = new JwtBearerEvents()
        {
          OnTokenValidated = c =>
          {            
            //Console.WriteLine($"User successfully authenticated,{c.Request.Path}");
            return Task.CompletedTask;
          },

          OnAuthenticationFailed = c =>
          {
            Console.WriteLine($"User authentication failed,{c.Request.Path}");

            c.Response.OnStarting(() =>
            {
              c.NoResult();
              c.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
              c.Response.ContentType = @"text/plain";
              return Task.CompletedTask;
            });
  
            return Task.CompletedTask;
          }
        };
      }
    }

    public void Configure(JwtBearerOptions options)
    {
      // default case: no scheme name was specified
      Configure(string.Empty, options);
    }
  }
}
