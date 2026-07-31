namespace TreviaApp.Application.TrainingPlans.Commands.ArchiveTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record ArchiveTrainingPlanCommand(Guid TrainingPlanId) : ICommand<TrainingPlanDetailResponse>;
