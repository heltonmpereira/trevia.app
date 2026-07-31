namespace TreviaApp.Application.TrainingPlans.Queries.GetTrainingPlansAssignedToStudent;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TrainingPlanStatus = TreviaApp.Shared.Enums.TrainingPlanStatus;

public sealed record GetTrainingPlansAssignedToStudentQuery(
    Guid StudentId,
    int Page = 1,
    int PageSize = 10,
    TrainingPlanStatus? StatusFilter = null)
    : IQuery<TrainingPlansSearchPagedResponse>;
