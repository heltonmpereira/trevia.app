namespace TreviaApp.Application.Security;

public interface IRefreshTokenStore
{
    Task StoreAsync(Guid userId, string tokenId, string tokenHash, DateTimeOffset expiresAt, string deviceInfo, string ipAddress, CancellationToken ct);
    Task RevokeByTokenIdAsync(string tokenId, string reason, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct);
    Task<RefreshTokenRecord?> GetByTokenIdAsync(string tokenId, CancellationToken ct);
    Task<List<RefreshTokenRecord>> GetActiveForUserAsync(Guid userId, CancellationToken ct);
    Task RotateAsync(string oldTokenId, Guid userId, string newTokenId, string newTokenHash, DateTimeOffset newExpiresAt, string deviceInfo, string ipAddress, CancellationToken ct);
}
public record RefreshTokenRecord(Guid UserId, string TokenId, string TokenHash, DateTimeOffset ExpiresAt, DateTimeOffset CreatedAt, string? DeviceInfo, string? IpAddress, bool IsRevoked, string? RevocationReason, DateTimeOffset? RevokedAt, string? ReplacedByTokenId);
