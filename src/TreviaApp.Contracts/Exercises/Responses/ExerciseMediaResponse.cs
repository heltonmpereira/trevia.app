using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Exercises.Responses;

/// <summary>
/// Response payload for ExerciseMediaResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="FileName">File Name value.</param>
/// <param name="MediaType">Media Type value.</param>
/// <param name="Order">Order value.</param>
/// <param name="Caption">Caption value.</param>
/// <param name="IsPrimary">Is Primary value.</param>
/// <param name="SizeBytes">Size Bytes value.</param>
/// <param name="AccessUrl">Access Url value.</param>
public sealed record ExerciseMediaResponse(
    Guid Id,
    string FileName,
    MediaType MediaType,
    int Order,
    string? Caption,
    bool IsPrimary,
    long SizeBytes,
    string? AccessUrl);
