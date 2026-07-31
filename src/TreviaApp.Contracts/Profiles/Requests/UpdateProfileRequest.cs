using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Profiles.Requests;

public sealed record UpdateProfileRequest(
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    PrivacyLevel PrivacyLevel,
    string PreferredUnits,
    string? Bio);
