namespace TreviaApp.Application.TrainingPlans.Queries.GetMyTrainingPlans;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TrainingPlanStatus = TreviaApp.Shared.Enums.TrainingPlanStatus;

public sealed record GetMyTrainingPlansQuery(
    int Page = 1,
    int PageSize = 10,
    TrainingPlanStatus? StatusFilter = null,
    string? SearchName = null,
    string? SortBy = "createdAtDesc")
    : IQuery<TrainingPlansSearchPagedResponse>;
