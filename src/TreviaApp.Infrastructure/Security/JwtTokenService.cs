namespace TreviaApp.Infrastructure.Security;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Shared.Constants;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _validationParameters = BuildValidationParameters();
    }

    public AuthResponse GenerateTokens(Guid userId, string email, IEnumerable<Claim> additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.UserId, userId.ToString()),
            new(ClaimTypes.Email, email)
        };

        claims.AddRange(additionalClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessExpires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var accessToken = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: accessExpires.UtcDateTime,
            signingCredentials: creds);

        var accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);

        var refreshTokenId = Guid.NewGuid().ToString("N");
        var refreshTokenBytes = RandomNumberGenerator.GetBytes(32);
        var refreshTokenStr = refreshTokenId + "." + Convert.ToBase64String(refreshTokenBytes);
        var refreshExpires = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);

        return new AuthResponse(
            accessTokenStr,
            accessExpires,
            refreshTokenStr,
            userId,
            email,
            claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList());
    }

    public ClaimsPrincipal? ValidateAccessToken(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(accessToken, _validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                return null;

            return principal;
        }
        catch { return null; }
    }

    public bool ValidateRefreshTokenProperties(ClaimsPrincipal principal, string storedTokenId, DateTimeOffset storedExpiresAt)
    {
        if (principal == null) return false;
        if (DateTimeOffset.UtcNow > storedExpiresAt) return false;
        return true;
    }

    private TokenValidationParameters BuildValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
        ClockSkew = TimeSpan.Zero
    };
}

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;
}
