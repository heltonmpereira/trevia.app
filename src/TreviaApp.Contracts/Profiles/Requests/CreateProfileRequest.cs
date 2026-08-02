using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Requests;

/// <summary>
/// Request payload for CreateProfileRequest.
/// </summary>
/// <param name="Goal">Goal value.</param>
/// <param name="Experience">Experience value.</param>
/// <param name="PreferredEnvironment">Preferred Environment value.</param>
/// <param name="PrivacyLevel">Privacy Level value.</param>
/// <param name="PreferredUnits">Preferred Units value.</param>
/// <param name="Bio">Bio value.</param>
public sealed record CreateProfileRequest(
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    PrivacyLevel? PrivacyLevel = null,
    string? PreferredUnits = null,
    string? Bio = null);
