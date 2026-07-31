using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record SessionExerciseResponse(
    Guid Id,
    int Order,
    Guid ExerciseId,
    string ExerciseName,
    string? ExerciseShortDescription,
    string? ExercisePhotoUrl,
    string? NotesForStudent,
    string? NotesForCoach,
    TimeSpan? DefaultRestBetweenSetsSeconds,
    SetTechnique? GlobalTechnique,
    IReadOnlyList<SetPrescriptionResponse> Prescriptions);
