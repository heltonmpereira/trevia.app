namespace TreviaApp.Application.TrainingPlans.Commands.RemoveExerciseFromSession;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record RemoveExerciseFromSessionCommand(
    Guid TrainingPlanId,
    Guid SessionId,
    Guid SessionExerciseId)
    : ICommand<TrainingPlanDetailResponse>;
