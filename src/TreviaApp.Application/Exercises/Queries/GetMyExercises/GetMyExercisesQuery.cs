namespace TreviaApp.Application.Exercises.Queries.GetMyExercises;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

public sealed record GetMyExercisesQuery(
    int Page = 1,
    int PageSize = 20,
    ExerciseStatus? Status = null)
    : IQuery<ExerciseSearchPagedResponse>;
