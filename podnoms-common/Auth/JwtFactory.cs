using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth;

public class JwtFactory : IJwtFactory {
  private readonly JwtIssuerOptions _jwtOptions;

  public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions) {
    _jwtOptions = jwtOptions.Value;
    ThrowIfInvalidOptions(_jwtOptions);
  }

  public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity, string[] roles) {
    var claims = new List<Claim> {
      new(JwtRegisteredClaimNames.Sub, userName),
      new(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
      new(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
      identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Rol),
      identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id)
    };
    AddRolesToClaims(claims, roles); // Create the JWT security token and encode it.
    var jwt = new JwtSecurityToken(
      _jwtOptions.Issuer,
      _jwtOptions.Audience,
      claims,
      _jwtOptions.NotBefore,
      //TODO: This is suboptimal but required until we figure how to do proper auth between
      //TODO: job server and SignalR hub
      _jwtOptions.Expiration,
      _jwtOptions.SigningCredentials);

    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    return encodedJwt;
  }

  public string DecodeToken(string token) {
    var secret = _jwtOptions.SigningKey;
    var key = Encoding.ASCII.GetBytes(secret);
    var handler = new JwtSecurityTokenHandler();
    var validations = new TokenValidationParameters {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = false,
      ValidateAudience = false
    };
    var parsedToken = handler.ReadJwtToken(token);
    return parsedToken.Subject;
  }

  public ClaimsIdentity GenerateClaimsIdentity(string userName, string id) {
    return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[] {
      new Claim(Constants.Strings.JwtClaimIdentifiers.Id, id),
      new Claim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess)
    });
  }

  private void AddRolesToClaims(List<Claim> claims, IEnumerable<string> roles) {
    foreach (var role in roles) {
      var roleClaim = new Claim(ClaimsIdentity.DefaultRoleClaimType, role);
      claims.Add(roleClaim);
    }
  }

  /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
  private static long ToUnixEpochDate(DateTime date) {
    return (long)Math.Round((date.ToUniversalTime() -
                             new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
      .TotalSeconds);
  }

  private static void ThrowIfInvalidOptions(JwtIssuerOptions options) {
    if (options is null) {
      throw new ArgumentNullException(nameof(options));
    }

    if (options.ValidFor <= TimeSpan.Zero) {
      throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
    }

    if (options.SigningCredentials is null) {
      throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
    }

    if (options.JtiGenerator is null) {
      throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
    }
  }
}
