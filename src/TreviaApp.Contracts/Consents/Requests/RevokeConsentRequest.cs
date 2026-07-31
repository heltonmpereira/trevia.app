using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Requests;

public sealed record RevokeConsentRequest(ConsentType ConsentType, string? Reason = null);
