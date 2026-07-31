namespace TreviaApp.Application.TrainingPlans.Commands.AddExerciseToTrainingSession;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Requests;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record AddExerciseToTrainingSessionCommand(
    Guid TrainingPlanId,
    Guid SessionId,
    Guid ExerciseId,
    int Order,
    string? NotesForStudent,
    string? NotesForCoach,
    TimeSpan? DefaultRestBetweenSetsSeconds,
    List<SetPrescriptionRequest>? InitialSets)
    : ICommand<TrainingPlanDetailResponse>;
