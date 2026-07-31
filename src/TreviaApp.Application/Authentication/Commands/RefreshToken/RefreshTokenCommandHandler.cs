namespace TreviaApp.Application.Authentication.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenStore _refreshStore;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(IJwtTokenService jwtService, IRefreshTokenStore refreshStore, UserManager<AppUser> userManager, ILogger<RefreshTokenCommandHandler> logger)
    {
        _jwtService = jwtService;
        _refreshStore = refreshStore;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.ValidateAccessToken(request.AccessToken);
        var userIdClaim = principal?.FindFirst(c => c.Type == AppClaimTypes.UserId || c.Type == ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim?.Value, out var userId))
            throw new DomainException("Token inválido.", ErrorCodes.RefreshTokenInvalid);

        var incomingTokenId = ExtractId(request.RefreshToken);
        var stored = await _refreshStore.GetByTokenIdAsync(incomingTokenId, cancellationToken);
        if (stored is null || stored.IsRevoked || !VerifyHash(request.RefreshToken, stored.TokenHash))
        {
            if (stored is not null)
            {
                await _refreshStore.RevokeAllForUserAsync(userId, "ReuseDetected", cancellationToken);
                _logger.LogWarning("RefreshTokenReuseDetected UserId={UserId}", userId);
            }
            throw new DomainException("Token inválido.", ErrorCodes.RefreshTokenInvalid);
        }

        if (stored.UserId != userId)
            throw new DomainException("Token não pertence ao usuário.", ErrorCodes.RefreshTokenInvalid);

        if (stored.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new DomainException("Refresh token expirado.", ErrorCodes.RefreshTokenExpired);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
            throw new DomainException("Usuário não encontrado.", ErrorCodes.RefreshTokenInvalid);

        user.LastActiveAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var claims = await BuildClaims(user);
        var auth = _jwtService.GenerateTokens(user.Id, user.Email!, claims);
        var newTokenId = ExtractId(auth.RefreshToken);
        var newTokenHash = ComputeHash(auth.RefreshToken);
        await _refreshStore.RotateAsync(incomingTokenId, userId, newTokenId, newTokenHash, auth.ExpiresAt.AddDays(7), stored.DeviceInfo ?? "api", "api-refresh", cancellationToken);

        _logger.LogInformation("TokenRefreshed UserId={UserId}", userId);
        return auth;
    }

    private async Task<List<Claim>> BuildClaims(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(AppClaimTypes.UserId, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;
    }

    private static string ExtractId(string refresh) => refresh.IndexOf('.') is var i && i > 0 ? refresh.Substring(0, i) : refresh;

    private static string ComputeHash(string s)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s)));
    }

    private static bool VerifyHash(string s, string storedHash) => ComputeHash(s) == storedHash;
}
