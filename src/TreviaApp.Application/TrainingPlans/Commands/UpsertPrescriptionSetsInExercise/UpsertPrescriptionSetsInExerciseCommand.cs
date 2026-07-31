namespace TreviaApp.Application.TrainingPlans.Commands.UpsertPrescriptionSetsInExercise;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Requests;
using TreviaApp.Contracts.TrainingPlans.Responses;

public sealed record UpsertPrescriptionSetsInExerciseCommand(
    Guid TrainingPlanId,
    Guid SessionId,
    Guid SessionExerciseId,
    List<SetPrescriptionRequest> Sets)
    : ICommand<TrainingPlanDetailResponse>;
