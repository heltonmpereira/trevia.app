namespace TreviaApp.Application.TrainingPlans.Commands.AddSessionToTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record AddSessionToTrainingPlanCommand(
    Guid TrainingPlanId,
    string Name,
    int Order,
    string? Description,
    DayOfWeek? SuggestedDayOfWeek,
    TimeSpan? EstimatedDuration,
    string? CoachNotesInternal,
    string? Focus)
    : ICommand<TrainingPlanDetailResponse>;
