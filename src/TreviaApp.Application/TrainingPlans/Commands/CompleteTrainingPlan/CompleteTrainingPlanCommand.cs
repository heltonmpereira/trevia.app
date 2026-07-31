namespace TreviaApp.Application.TrainingPlans.Commands.CompleteTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record CompleteTrainingPlanCommand(Guid TrainingPlanId) : ICommand<TrainingPlanDetailResponse>;
