using MediatR;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Errors;

namespace TreviaApp.Application.WorkoutExecution.Commands.StartWorkoutSession;

public sealed record StartWorkoutSessionCommand(
    Guid CurrentUserId,
    Guid TrainingSessionId,
    int WeekNumberInPlan = 1)
    : IRequest<Result<WorkoutSessionSummaryResponse>>;
