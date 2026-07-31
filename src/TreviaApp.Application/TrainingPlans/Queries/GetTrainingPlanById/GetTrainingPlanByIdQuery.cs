namespace TreviaApp.Application.TrainingPlans.Queries.GetTrainingPlanById;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record GetTrainingPlanByIdQuery(Guid TrainingPlanId) : IQuery<TrainingPlanDetailResponse>;
