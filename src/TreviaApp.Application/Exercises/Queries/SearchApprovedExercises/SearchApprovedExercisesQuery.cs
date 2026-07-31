namespace TreviaApp.Application.Exercises.Queries.SearchApprovedExercises;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;

public sealed record SearchApprovedExercisesQuery(SearchExercisesRequest Request)
    : IQuery<ExerciseSearchPagedResponse>;
