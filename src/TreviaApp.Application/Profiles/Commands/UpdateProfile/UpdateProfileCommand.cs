namespace TreviaApp.Application.Profiles.Commands.UpdateProfile;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Shared.Enums;

public sealed record UpdateProfileCommand(
    PrivacyLevel PrivacyLevel,
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    string PreferredUnits,
    string? Bio) : ICommand<ProfileFullResponse>;
