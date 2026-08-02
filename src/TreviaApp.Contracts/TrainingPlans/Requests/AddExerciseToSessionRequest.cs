namespace TreviaApp.Contracts.TrainingPlans.Requests;

/// <summary>
/// Request payload for AddExerciseToSessionRequest.
/// </summary>
/// <param name="ExerciseId">Exercise Id value.</param>
/// <param name="Order">Order value.</param>
/// <param name="NotesForStudent">Notes For Student value.</param>
/// <param name="NotesForCoach">Notes For Coach value.</param>
/// <param name="DefaultRestBetweenSetsSeconds">Default Rest Between Sets Seconds value.</param>
/// <param name="InitialSets">Initial Sets value.</param>
public sealed record AddExerciseToSessionRequest(
    Guid ExerciseId,
    int Order,
    string? NotesForStudent = null,
    string? NotesForCoach = null,
    TimeSpan? DefaultRestBetweenSetsSeconds = null,
    List<SetPrescriptionRequest>? InitialSets = null);
