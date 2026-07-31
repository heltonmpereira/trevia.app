namespace TreviaApp.Application.TrainingPlans.Commands.AssignToStudentTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record AssignToStudentTrainingPlanCommand(
    Guid TrainingPlanId,
    Guid StudentId)
    : ICommand<TrainingPlanDetailResponse>;
