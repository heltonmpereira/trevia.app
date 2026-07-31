using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Requests;

public sealed record CreateProfileRequest(
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    PrivacyLevel? PrivacyLevel = null,
    string? PreferredUnits = null,
    string? Bio = null);
