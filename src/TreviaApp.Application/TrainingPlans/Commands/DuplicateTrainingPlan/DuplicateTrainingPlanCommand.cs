namespace TreviaApp.Application.TrainingPlans.Commands.DuplicateTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record DuplicateTrainingPlanCommand(
    Guid TrainingPlanId,
    string? NewName,
    bool AssignToMe)
    : ICommand<TrainingPlanDetailResponse>;
