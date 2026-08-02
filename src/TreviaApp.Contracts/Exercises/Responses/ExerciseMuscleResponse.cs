using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

/// <summary>
/// Response payload for ExerciseMuscleResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Muscle">Muscle value.</param>
/// <param name="MuscleRole">Muscle Role value.</param>
/// <param name="ActivationPercent">Activation Percent value.</param>
public sealed record ExerciseMuscleResponse(
    Guid Id,
    Muscle Muscle,
    MuscleRole MuscleRole,
    decimal? ActivationPercent);
