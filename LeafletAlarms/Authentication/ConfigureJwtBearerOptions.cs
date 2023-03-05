using DbLayer.Services;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace LeafletAlarms.Authentication
{
  public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
  {
    private readonly KeyCloakConnectorService _myService;

    public ConfigureJwtBearerOptions(KeyCloakConnectorService myService)
    {
      _myService = myService;
    }
    public void Configure(string name, JwtBearerOptions o)
    {
      // check that we are currently configuring the options for the correct scheme
      if (name == JwtBearerDefaults.AuthenticationScheme)
      {
        //Realm settings/Keys/RS256(public)
        var publicKeyJWT = _myService.GetRealmInfo().Result.public_key;

        var realmName = _myService.GetRealmName();

        var BaseAddr = _myService.GetBaseAddr();

        //"http://localhost:8080/realms/myrealm"
        Uri? validIssuer;
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
          IssuerSigningKey = ConfigureAuthentificationServiceExtensions.BuildRSAKey(publicKeyJWT),
          ValidateLifetime = true
        };

        o.Events = new JwtBearerEvents()
        {
          OnTokenValidated = c =>
          {
            Console.WriteLine("User successfully authenticated");
            return Task.CompletedTask;
          },
          OnAuthenticationFailed = c =>
          {
            c.NoResult();

            c.Response.StatusCode = 500;
            c.Response.ContentType = "text/plain";

            return c.Response.WriteAsync(c.Exception.ToString());

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
