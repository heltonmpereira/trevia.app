namespace TreviaApp.Application.TrainingPlans.Commands.ReorderExercisesInSession;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record ReorderExercisesInSessionCommand(
    Guid TrainingPlanId,
    Guid SessionId,
    List<(Guid SessionExerciseId, int NewOrder)> Orders)
    : ICommand<TrainingPlanDetailResponse>;
