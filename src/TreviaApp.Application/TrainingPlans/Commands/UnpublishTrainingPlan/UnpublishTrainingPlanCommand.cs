namespace TreviaApp.Application.TrainingPlans.Commands.UnpublishTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record UnpublishTrainingPlanCommand(Guid TrainingPlanId) : ICommand<TrainingPlanDetailResponse>;
