
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace LeafletAlarms.Authentication
{
  public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
  {
    private readonly KeyCloakConnectorService _myService;
    private RsaSecurityKey _securityKey;

    public ConfigureJwtBearerOptions(KeyCloakConnectorService myService)
    {
      _myService = myService;
    }

    private RsaSecurityKey GetKey()
    {
      if (_securityKey != null)
      {
        return _securityKey;
      }

      //Realm settings/Keys/RS256(public)
      string publicKeyJWT = string.Empty;

      publicKeyJWT = _myService.GetRealmInfo().Result?.public_key;

      if (string.IsNullOrEmpty(publicKeyJWT))
      {
        Console.WriteLine($"get publicKeyJWT: FAILED");
        return null;
      }
      _securityKey = ConfigureAuthentificationServiceExtensions.BuildRSAKey(publicKeyJWT);
      return _securityKey;
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
          return new List<SecurityKey>()
          {
            GetKey()
          };
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
