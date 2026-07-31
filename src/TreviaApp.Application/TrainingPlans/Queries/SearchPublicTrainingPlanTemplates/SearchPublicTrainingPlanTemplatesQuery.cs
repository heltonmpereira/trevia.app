namespace TreviaApp.Application.TrainingPlans.Queries.SearchPublicTrainingPlanTemplates;

using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Shared.Enums;
using DifficultyLevel = TreviaApp.Shared.Enums.DifficultyLevel;
using TrainingSplitType = TreviaApp.Shared.Enums.TrainingSplitType;

public sealed record SearchPublicTrainingPlanTemplatesQuery(
    int Page = 1,
    int PageSize = 12,
    string? SearchName = null,
    TrainingSplitType? SplitType = null,
    DifficultyLevel? Difficulty = null,
    int? MinSessions = null,
    string? SortBy = "popularity")
    : IQuery<TrainingPlansSearchPagedResponse>;
