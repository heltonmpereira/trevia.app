using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Consents.Responses;

public sealed record ConsentVersionInfoResponse(ConsentType ConsentType, string CurrentVersion, DateTimeOffset ReleasedAt);
