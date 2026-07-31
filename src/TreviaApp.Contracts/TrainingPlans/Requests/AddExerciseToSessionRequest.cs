namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record AddExerciseToSessionRequest(
    Guid ExerciseId,
    int Order,
    string? NotesForStudent = null,
    string? NotesForCoach = null,
    TimeSpan? DefaultRestBetweenSetsSeconds = null,
    List<SetPrescriptionRequest>? InitialSets = null);
