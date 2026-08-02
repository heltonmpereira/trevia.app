using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.WorkoutExecution.Responses;

namespace TreviaApp.Application.WorkoutExecution.Commands.StartWorkoutSession;

public sealed record StartWorkoutSessionCommand(
    Guid CurrentUserId,
    Guid TrainingSessionId,
    int WeekNumberInPlan = 1)
    : ICommand<WorkoutSessionSummaryResponse>;
