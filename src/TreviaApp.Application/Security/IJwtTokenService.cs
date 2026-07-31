namespace TreviaApp.Application.Security;
using System.Security.Claims;
using TreviaApp.Contracts.Authentication;

public interface IJwtTokenService
{
    AuthResponse GenerateTokens(Guid userId, string email, IEnumerable<Claim> additionalClaims);
    ClaimsPrincipal? ValidateAccessToken(string accessToken);
    bool ValidateRefreshTokenProperties(ClaimsPrincipal principal, string storedTokenId, DateTimeOffset storedExpiresAt);
}
