using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

/// <summary>
/// Request payload for AddMuscleToExerciseRequest.
/// </summary>
/// <param name="Muscle">Muscle value.</param>
/// <param name="Role">Role value.</param>
/// <param name="ActivationPercent">Activation Percent value.</param>
public sealed record AddMuscleToExerciseRequest(Muscle Muscle, MuscleRole Role = MuscleRole.Primary, decimal? ActivationPercent = null);
