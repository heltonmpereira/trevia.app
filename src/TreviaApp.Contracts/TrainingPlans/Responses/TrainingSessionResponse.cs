namespace TreviaApp.Contracts.TrainingPlans.Responses;

/// <summary>
/// Detailed response for a training session.
/// </summary>
/// <param name="Id">Training session identifier.</param>
/// <param name="Name">Training session name.</param>
/// <param name="Order">Display order within the plan.</param>
/// <param name="Description">Training session description, when available.</param>
/// <param name="SuggestedDayOfWeek">Suggested day of week, when available.</param>
/// <param name="EstimatedDurationMin">Estimated session duration, when available.</param>
/// <param name="CoachNotesInternal">Coach internal notes, when available.</param>
/// <param name="Focus">Session focus, when available.</param>
/// <param name="Exercises">Exercises configured for the session.</param>
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
