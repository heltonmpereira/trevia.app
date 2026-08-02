using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

/// <summary>
/// Request payload for SearchExercisesRequest.
/// </summary>
/// <param name="Name">Name value.</param>
/// <param name="Environment">Environment value.</param>
/// <param name="Modality">Modality value.</param>
/// <param name="DifficultyLevel">Difficulty Level value.</param>
/// <param name="PrimaryMuscle">Primary Muscle value.</param>
/// <param name="Equipment">Equipment value.</param>
/// <param name="MeasurementType">Measurement Type value.</param>
/// <param name="Page">Page value.</param>
/// <param name="PageSize">Page Size value.</param>
/// <param name="SortBy">Sort By value.</param>
/// <param name="SortDescending">Sort Descending value.</param>
public sealed record SearchExercisesRequest(
    string? Name = null,
    TrainingEnvironment? Environment = null,
    ExerciseModality? Modality = null,
    DifficultyLevel? DifficultyLevel = null,
    Muscle? PrimaryMuscle = null,
    Equipment? Equipment = null,
    MeasurementType? MeasurementType = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false);
