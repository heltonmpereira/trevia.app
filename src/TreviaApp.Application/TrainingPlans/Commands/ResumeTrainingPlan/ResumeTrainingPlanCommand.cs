namespace TreviaApp.Application.TrainingPlans.Commands.ResumeTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record ResumeTrainingPlanCommand(Guid TrainingPlanId) : ICommand<TrainingPlanDetailResponse>;
