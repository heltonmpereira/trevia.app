namespace TreviaApp.Application.TrainingPlans.Commands.PauseTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record PauseTrainingPlanCommand(Guid TrainingPlanId) : ICommand<TrainingPlanDetailResponse>;
