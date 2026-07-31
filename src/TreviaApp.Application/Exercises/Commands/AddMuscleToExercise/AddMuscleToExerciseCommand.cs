namespace TreviaApp.Application.Exercises.Commands.AddMuscleToExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

public sealed record AddMuscleToExerciseCommand(
    Guid ExerciseId,
    Muscle Muscle,
    MuscleRole Role = MuscleRole.Primary,
    decimal? ActivationPercent = null)
    : ICommand<ExerciseMuscleResponse>;
