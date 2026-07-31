namespace TreviaApp.Application.Exercises.Commands.AddMediaToExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

public sealed record AddMediaToExerciseCommand(
    Guid ExerciseId,
    byte[] FileBytes,
    string FileName,
    string ContentType,
    long SizeBytes,
    MediaType MediaType,
    int Order,
    string? Caption = null,
    bool IsPrimary = false)
    : ICommand<ExerciseMediaResponse>;
