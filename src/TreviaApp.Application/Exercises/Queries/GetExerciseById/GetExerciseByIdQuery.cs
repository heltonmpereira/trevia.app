namespace TreviaApp.Application.Exercises.Queries.GetExerciseById;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Exercises.Responses;

public sealed record GetExerciseByIdQuery(Guid ExerciseId) : IQuery<ExerciseDetailResponse>;
