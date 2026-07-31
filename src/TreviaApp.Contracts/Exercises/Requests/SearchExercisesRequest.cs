using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Requests;

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
