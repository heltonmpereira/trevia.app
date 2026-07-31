namespace TreviaApp.Application.TrainingPlans.Commands.UpdateExerciseInSession;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Shared.Enums;

public sealed record UpdateExerciseInSessionCommand(
    Guid TrainingPlanId,
    Guid SessionId,
    Guid SessionExerciseId,
    int Order,
    string? NotesForStudent,
    string? NotesForCoach,
    TimeSpan? DefaultRestBetweenSetsSeconds,
    SetTechnique? GlobalTechnique)
    : ICommand<TrainingPlanDetailResponse>;
