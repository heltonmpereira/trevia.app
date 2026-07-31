using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Requests;

public sealed record UpdateExerciseInSessionRequest(
    int Order,
    string? NotesForStudent,
    string? NotesForCoach,
    TimeSpan? DefaultRestBetweenSetsSeconds,
    SetTechnique? GlobalTechnique = null);
