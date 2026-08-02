using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Requests;

/// <summary>
/// Request payload for UpdateProfileRequest.
/// </summary>
/// <param name="Goal">Goal value.</param>
/// <param name="Experience">Experience value.</param>
/// <param name="PreferredEnvironment">Preferred Environment value.</param>
/// <param name="PrivacyLevel">Privacy Level value.</param>
/// <param name="PreferredUnits">Preferred Units value.</param>
/// <param name="Bio">Bio value.</param>
public sealed record UpdateProfileRequest(
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    PrivacyLevel PrivacyLevel,
    string PreferredUnits,
    string? Bio);
