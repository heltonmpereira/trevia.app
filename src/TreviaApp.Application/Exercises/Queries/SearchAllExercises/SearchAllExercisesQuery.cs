namespace TreviaApp.Application.Exercises.Queries.SearchAllExercises;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;

public sealed record SearchAllExercisesQuery(
    SearchExercisesRequest Filters,
    bool IncludeDeleted = false)
    : IQuery<ExerciseSearchPagedResponse>;
