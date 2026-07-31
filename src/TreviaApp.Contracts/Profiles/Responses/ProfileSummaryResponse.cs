using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record ProfileSummaryResponse(
    Guid Id,
    Guid UserId,
    string? DisplayName,
    string? Bio,
    TrainingGoal Goal,
    ExperienceLevel Experience,
    ProfilePhotoResponse? Photo,
    PrivacyLevel PrivacyLevel);
