namespace TreviaApp.Application.Profiles.Commands.CreateProfile;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Shared.Enums;

public sealed record CreateProfileCommand(
    TrainingGoal Goal,
    ExperienceLevel Experience,
    TrainingEnvironment PreferredEnvironment,
    PrivacyLevel PrivacyLevel,
    string PreferredUnits,
    string? Bio) : ICommand<ProfileFullResponse>;
