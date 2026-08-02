using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Responses;

/// <summary>
/// Response payload for ConsentVersionInfoResponse.
/// </summary>
/// <param name="ConsentType">Consent Type value.</param>
/// <param name="CurrentVersion">Current Version value.</param>
/// <param name="ReleasedAt">Released At value.</param>
public sealed record ConsentVersionInfoResponse(ConsentType ConsentType, string CurrentVersion, DateTimeOffset ReleasedAt);
