namespace TreviaApp.Infrastructure.Security;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Security;
using TreviaApp.Infrastructure.Identity;
using TreviaApp.Infrastructure.Persistence;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenStore(ApplicationDbContext context) => _context = context;

    public async Task<List<RefreshTokenRecord>> GetActiveForUserAsync(Guid userId, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToRecord(t))
            .ToListAsync(ct);
    }

    public async Task<RefreshTokenRecord?> GetByTokenIdAsync(string tokenId, CancellationToken ct)
    {
        var t = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenId == tokenId, ct);
        return t == null ? null : ToRecord(t);
    }

    public async Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct)
    {
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync(ct);
        foreach (var t in tokens) { t.IsRevoked = true; t.RevocationReason = reason; t.RevokedAt = DateTimeOffset.UtcNow; }
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeByTokenIdAsync(string tokenId, string reason, CancellationToken ct)
    {
        var t = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenId == tokenId, ct);
        if (t != null) { t.IsRevoked = true; t.RevocationReason = reason; t.RevokedAt = DateTimeOffset.UtcNow; await _context.SaveChangesAsync(ct); }
    }

    public async Task RotateAsync(
        string oldTokenId,
        Guid userId,
        string newTokenId,
        string newTokenHash,
        DateTimeOffset newExpiresAt,
        string deviceInfo,
        string ipAddress,
        CancellationToken ct)
    {
        var old = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenId == oldTokenId, ct);
        if (old != null) { old.IsRevoked = true; old.RevokedAt = DateTimeOffset.UtcNow; old.RevocationReason = "Rotated"; old.ReplacedByTokenId = newTokenId; }

        await _context.RefreshTokens.AddAsync(new RefreshToken
        {
            TokenId = newTokenId,
            TokenHash = newTokenHash,
            UserId = userId,
            ExpiresAt = newExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        }, ct);

        await _context.SaveChangesAsync(ct);
    }

    public async Task StoreAsync(
        Guid userId,
        string tokenId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string deviceInfo,
        string ipAddress,
        CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(new RefreshToken
        {
            TokenId = tokenId,
            TokenHash = tokenHash,
            UserId = userId,
            ExpiresAt = expiresAt,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        }, ct);

        await _context.SaveChangesAsync(ct);
    }

    private static RefreshTokenRecord ToRecord(RefreshToken t) =>
        new(t.UserId, t.TokenId, t.TokenHash, t.ExpiresAt, t.CreatedAt, t.DeviceInfo, t.IpAddress, t.IsRevoked, t.RevocationReason, t.RevokedAt, t.ReplacedByTokenId);
}
