using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OsmSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeafletAlarms.Authentication
{
  public class RootRoles
  {
    public List<string> roles { get; set; }
  }

  /// <summary>
  /// Used to get the role within the claims structure used by keycloak, then it adds the role(s) in the ClaimsItentity of ClaimsPrincipal.Identity
  /// </summary>
  public class ClaimsTransformer : IClaimsTransformation
  {
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
      ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;

      // flatten resource_access because Microsoft identity model doesn't support nested claims
      // by map it to Microsoft identity model, because automatic JWT bearer token mapping already processed here
      if (claimsIdentity.IsAuthenticated &&
        claimsIdentity.HasClaim((claim) => claim.Type == "realm_access"))
      {
        var userRole = claimsIdentity.FindFirst((claim) => claim.Type == "realm_access");
        var userName = claimsIdentity
          .FindFirst((claim) => claim.Type == "preferred_username")
          ?.Value;

        var content = JsonSerializer.Deserialize<RootRoles>(userRole.Value);
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "anon"));

        if (!string.IsNullOrEmpty(userName))
        {
          claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
        }

        foreach (var role in content.roles)
        {
          claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
        }
      }

      return Task.FromResult(principal);
    }
  }

  public static class ConfigureAuthentificationServiceExtensions
  {
    public static RsaSecurityKey BuildRSAKey(string publicKeyJWT)
    {
      RSA rsa = RSA.Create();

      rsa.ImportSubjectPublicKeyInfo(

          source: Convert.FromBase64String(publicKeyJWT),
          bytesRead: out _
      );

      var IssuerSigningKey = new RsaSecurityKey(rsa);

      return IssuerSigningKey;
    }

    public static void ConfigureJWT(
      this IServiceCollection services
    )
    {
      services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
      services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

      var AuthenticationBuilder = services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
      });
      AuthenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions: null);
    }
  }
}
