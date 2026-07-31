using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Identity;

public class UserConsent : Entity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public ConsentType ConsentType { get; private set; }
    public string ConsentVersion { get; private set; } = null!;

    public DateTimeOffset AcceptedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    private UserConsent() { }

    public UserConsent(Guid userId, ConsentType consentType, string consentVersion,
                       string? ipAddress = null, string? userAgent = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
        if (string.IsNullOrWhiteSpace(consentVersion)) throw new ArgumentException(nameof(consentVersion));

        UserId = userId;
        ConsentType = consentType;
        ConsentVersion = consentVersion;
        AcceptedAt = DateTimeOffset.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(string? reason = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        RevocationReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
