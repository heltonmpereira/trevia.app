using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Requests;

public sealed record GiveConsentBatchRequest(IEnumerable<GiveConsentRequest> Consents);
