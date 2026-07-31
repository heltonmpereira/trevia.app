using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

public sealed record ExerciseMediaResponse(
    Guid Id,
    string FileName,
    MediaType MediaType,
    int Order,
    string? Caption,
    bool IsPrimary,
    long SizeBytes,
    string? AccessUrl);
