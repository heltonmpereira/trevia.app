namespace TreviaApp.Application.TrainingPlans.Commands.UpdateTrainingPlan;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Shared.Enums;

public sealed record UpdateTrainingPlanCommand(
    Guid TrainingPlanId,
    string Name,
    string? Description,
    string? InstructionsIntro,
    string? NotesForStudent,
    TrainingSplitType SplitType,
    Visibility Visibility,
    int? TotalWeeks,
    int? SessionsPerWeek,
    decimal? TargetVolume,
    string? Tags)
    : ICommand<TrainingPlanDetailResponse>;
