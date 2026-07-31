namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record UpdateTrainingSessionRequest(
    Guid Id,
    string Name,
    int Order,
    string? Description,
    DayOfWeek? SuggestedDayOfWeek,
    TimeSpan? EstimatedDuration,
    string? CoachNotesInternal,
    string? Focus);
