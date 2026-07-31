using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Responses;

public sealed record ConsentResponse(
    Guid Id,
    ConsentType ConsentType,
    string ConsentVersion,
    DateTimeOffset AcceptedAt,
    bool IsRevoked,
    DateTimeOffset? RevokedAt,
    string? RevocationReason);
