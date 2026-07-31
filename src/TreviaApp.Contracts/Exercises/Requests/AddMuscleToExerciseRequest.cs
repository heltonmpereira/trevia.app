using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

public sealed record AddMuscleToExerciseRequest(Muscle Muscle, MuscleRole Role = MuscleRole.Primary, decimal? ActivationPercent = null);
