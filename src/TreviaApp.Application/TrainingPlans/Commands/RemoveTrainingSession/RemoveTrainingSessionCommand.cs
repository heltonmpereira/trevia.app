namespace TreviaApp.Application.TrainingPlans.Commands.RemoveTrainingSession;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record RemoveTrainingSessionCommand(
    Guid TrainingPlanId,
    Guid SessionId)
    : ICommand<TrainingPlanDetailResponse>;
