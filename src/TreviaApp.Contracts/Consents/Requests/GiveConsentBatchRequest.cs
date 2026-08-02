using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Requests;

/// <summary>
/// Request payload for GiveConsentBatchRequest.
/// </summary>
/// <param name="Consents">Consents value.</param>
public sealed record GiveConsentBatchRequest(IEnumerable<GiveConsentRequest> Consents);
