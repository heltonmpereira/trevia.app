namespace TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record TrainingSessionResponse(
    Guid Id,
    string Name,
    int Order,
    string? Description,
    DayOfWeek? SuggestedDayOfWeek,
    TimeSpan? EstimatedDurationMin,
    string? CoachNotesInternal,
    string? Focus,
    IReadOnlyList<SessionExerciseResponse> Exercises);
