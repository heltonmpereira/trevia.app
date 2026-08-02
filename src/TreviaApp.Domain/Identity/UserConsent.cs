using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Identity;

/// <summary>
/// Represents the UserConsent domain entity.
/// </summary>
public class UserConsent : Entity
{
    /// <summary>
    /// Gets User Id.
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// Gets User.
    /// </summary>
    public AppUser User { get; private set; } = null!;

    /// <summary>
    /// Gets Consent Type.
    /// </summary>
    public ConsentType ConsentType { get; private set; }
    /// <summary>
    /// Gets Consent Version.
    /// </summary>
    public string ConsentVersion { get; private set; } = null!;

    /// <summary>
    /// Gets Accepted At.
    /// </summary>
    public DateTimeOffset AcceptedAt { get; private set; }
    /// <summary>
    /// Gets Ip Address.
    /// </summary>
    public string? IpAddress { get; private set; }
    /// <summary>
    /// Gets User Agent.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Gets Revoked At.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }
    /// <summary>
    /// Gets Revocation Reason.
    /// </summary>
    public string? RevocationReason { get; private set; }

    /// <summary>
    /// Gets Is Revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    private UserConsent() { }

    /// <summary>
    /// Initializes a new instance of the UserConsent class.
    /// </summary>
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

    /// <summary>
    /// Executes Revoke.
    /// </summary>
    public void Revoke(string? reason = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        RevocationReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
