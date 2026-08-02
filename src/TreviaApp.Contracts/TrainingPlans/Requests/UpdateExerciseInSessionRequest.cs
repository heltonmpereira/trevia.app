using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for UpdateExerciseInSessionRequest.
/// </summary>
/// <param name="Order">Order value.</param>
/// <param name="NotesForStudent">Notes For Student value.</param>
/// <param name="NotesForCoach">Notes For Coach value.</param>
/// <param name="DefaultRestBetweenSetsSeconds">Default Rest Between Sets Seconds value.</param>
/// <param name="GlobalTechnique">Global Technique value.</param>
public sealed record UpdateExerciseInSessionRequest(
    int Order,
    string? NotesForStudent,
    string? NotesForCoach,
    TimeSpan? DefaultRestBetweenSetsSeconds,
    SetTechnique? GlobalTechnique = null);
