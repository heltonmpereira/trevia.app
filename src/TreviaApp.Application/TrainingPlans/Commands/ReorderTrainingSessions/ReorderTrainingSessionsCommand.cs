namespace TreviaApp.Application.TrainingPlans.Commands.ReorderTrainingSessions;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record ReorderTrainingSessionsCommand(
    Guid TrainingPlanId,
    List<(Guid SessionId, int NewOrder)> Orders)
    : ICommand<TrainingPlanDetailResponse>;
