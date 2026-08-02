using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Responses;

/// <summary>
/// Response payload for ConsentResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="ConsentType">Consent Type value.</param>
/// <param name="ConsentVersion">Consent Version value.</param>
/// <param name="AcceptedAt">Accepted At value.</param>
/// <param name="IsRevoked">Is Revoked value.</param>
/// <param name="RevokedAt">Revoked At value.</param>
/// <param name="RevocationReason">Revocation Reason value.</param>
public sealed record ConsentResponse(
    Guid Id,
    ConsentType ConsentType,
    string ConsentVersion,
    DateTimeOffset AcceptedAt,
    bool IsRevoked,
    DateTimeOffset? RevokedAt,
    string? RevocationReason);
