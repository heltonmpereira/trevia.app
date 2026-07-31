using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Requests;

public sealed record GiveConsentRequest(ConsentType ConsentType, string ConsentVersion);
