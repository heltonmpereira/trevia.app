namespace TreviaApp.Application.TrainingPlans.Commands.PublishTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record PublishTrainingPlanCommand(
    Guid TrainingPlanId,
    bool AsPublicTemplate)
    : ICommand<TrainingPlanDetailResponse>;
