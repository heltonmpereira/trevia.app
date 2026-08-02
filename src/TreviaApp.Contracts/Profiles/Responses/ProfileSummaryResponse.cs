using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for ProfileSummaryResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="UserId">User Id value.</param>
/// <param name="DisplayName">Display Name value.</param>
/// <param name="Bio">Bio value.</param>
/// <param name="Goal">Goal value.</param>
/// <param name="Experience">Experience value.</param>
/// <param name="Photo">Photo value.</param>
/// <param name="PrivacyLevel">Privacy Level value.</param>
public sealed record ProfileSummaryResponse(
    Guid Id,
    Guid UserId,
    string? DisplayName,
    string? Bio,
    TrainingGoal Goal,
    ExperienceLevel Experience,
    ProfilePhotoResponse? Photo,
    PrivacyLevel PrivacyLevel);
