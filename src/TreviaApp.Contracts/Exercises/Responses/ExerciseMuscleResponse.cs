using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

public sealed record ExerciseMuscleResponse(
    Guid Id,
    Muscle Muscle,
    MuscleRole MuscleRole,
    decimal? ActivationPercent);
