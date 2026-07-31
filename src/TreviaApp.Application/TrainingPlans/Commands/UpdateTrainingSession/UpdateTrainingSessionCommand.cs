namespace TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingSession;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record UpdateTrainingSessionCommand(
    Guid TrainingPlanId,
    Guid SessionId,
    string Name,
    int Order,
    string? Description,
    DayOfWeek? SuggestedDayOfWeek,
    TimeSpan? EstimatedDuration,
    string? CoachNotesInternal,
    string? Focus)
    : ICommand<TrainingPlanDetailResponse>;
