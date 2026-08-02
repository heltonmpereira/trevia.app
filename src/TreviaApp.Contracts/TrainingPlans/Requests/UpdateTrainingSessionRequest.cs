namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for UpdateTrainingSessionRequest.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Name">Name value.</param>
/// <param name="Order">Order value.</param>
/// <param name="Description">Description value.</param>
/// <param name="SuggestedDayOfWeek">Suggested Day Of Week value.</param>
/// <param name="EstimatedDuration">Estimated Duration value.</param>
/// <param name="CoachNotesInternal">Coach Notes Internal value.</param>
/// <param name="Focus">Focus value.</param>
public sealed record UpdateTrainingSessionRequest(
    Guid Id,
    string Name,
    int Order,
    string? Description,
    DayOfWeek? SuggestedDayOfWeek,
    TimeSpan? EstimatedDuration,
    string? CoachNotesInternal,
    string? Focus);
