using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Requests;

/// <summary>
/// Request payload for GiveConsentRequest.
/// </summary>
/// <param name="ConsentType">Consent Type value.</param>
/// <param name="ConsentVersion">Consent Version value.</param>
public sealed record GiveConsentRequest(ConsentType ConsentType, string ConsentVersion);
