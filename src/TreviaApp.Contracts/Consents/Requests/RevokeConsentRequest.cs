using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Requests;

/// <summary>
/// Request payload for RevokeConsentRequest.
/// </summary>
/// <param name="ConsentType">Consent Type value.</param>
/// <param name="Reason">Reason value.</param>
public sealed record RevokeConsentRequest(ConsentType ConsentType, string? Reason = null);
