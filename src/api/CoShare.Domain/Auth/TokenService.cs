using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoShare.Domain.Auth;

/// <summary>
/// JWT token generation service.
/// </summary>
public interface ITokenService
{
    (string AccessToken, int ExpiresIn) GenerateAccessToken(long userId, string phone, int tier, long companyId);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
}

public class TokenService : ITokenService
{
    private readonly AuthOptions _options;

    public TokenService(IOptions<AuthOptions> options)
    {
        _options = options.Value;
    }

    public (string AccessToken, int ExpiresIn) GenerateAccessToken(
        long userId, string phone, int tier, long companyId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("phone", phone),
            new Claim("tier", tier.ToString()),
            new Claim("companyId", companyId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.JwtIssuer,
            audience: _options.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_options.AccessTokenExpirySeconds),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, _options.AccessTokenExpirySeconds);
    }

    public string GenerateRefreshToken() => OtpHelper.GenerateRefreshToken();

    public string HashRefreshToken(string token) => OtpHelper.HashRefreshToken(token);
}
