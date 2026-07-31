namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record AddTrainingSessionRequest(
    string Name,
    int Order,
    string? Description = null,
    DayOfWeek? SuggestedDayOfWeek = null,
    TimeSpan? EstimatedDuration = null,
    string? CoachNotesInternal = null,
    string? Focus = null);
