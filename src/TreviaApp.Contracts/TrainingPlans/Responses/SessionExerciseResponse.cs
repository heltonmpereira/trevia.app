using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.TrainingPlans.Responses;

/// <summary>
/// Response payload for SessionExerciseResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="Order">Order value.</param>
/// <param name="ExerciseId">Exercise Id value.</param>
/// <param name="ExerciseName">Exercise Name value.</param>
/// <param name="ExerciseShortDescription">Exercise Short Description value.</param>
/// <param name="ExercisePhotoUrl">Exercise Photo Url value.</param>
/// <param name="NotesForStudent">Notes For Student value.</param>
/// <param name="NotesForCoach">Notes For Coach value.</param>
/// <param name="DefaultRestBetweenSetsSeconds">Default Rest Between Sets Seconds value.</param>
/// <param name="GlobalTechnique">Global Technique value.</param>
/// <param name="Prescriptions">Prescriptions value.</param>
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
