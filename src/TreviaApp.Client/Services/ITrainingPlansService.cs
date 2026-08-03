using TreviaApp.Contracts.TrainingPlans.Requests;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public interface ITrainingPlansService
{
    Task<TrainingPlanDetailResponse> Create(CreateTrainingPlanRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Update(Guid planId, UpdateTrainingPlanRequest request, CancellationToken ct = default);
    Task Delete(Guid planId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Duplicate(Guid planId, DuplicatePlanRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Publish(Guid planId, PublishPlanRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Unpublish(Guid planId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> AssignToStudent(Guid planId, Guid studentId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Pause(Guid planId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Resume(Guid planId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Complete(Guid planId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> Archive(Guid planId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> AddSession(Guid planId, AddTrainingSessionRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> UpdateSession(Guid planId, Guid sessionId, UpdateTrainingSessionRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> RemoveSession(Guid planId, Guid sessionId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> ReorderSessions(Guid planId, ReorderSessionsRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> AddExerciseToSession(Guid planId, Guid sessionId, AddExerciseToSessionRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> UpdateExerciseInSession(Guid planId, Guid sessionId, Guid sessionExerciseId, UpdateExerciseInSessionRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> RemoveExerciseFromSession(Guid planId, Guid sessionId, Guid sessionExerciseId, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> ReorderExercisesInSession(Guid planId, Guid sessionId, ReorderExercisesInSessionRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> UpsertPrescriptionSets(Guid planId, Guid sessionId, Guid sessionExerciseId, UpsertPrescriptionSetsRequest request, CancellationToken ct = default);
    Task<TrainingPlanDetailResponse> GetById(Guid planId, CancellationToken ct = default);
    Task<TrainingPlansSearchPagedResponse> GetMyPlans(
        int page = 1,
        int pageSize = 10,
        TrainingPlanStatus? statusFilter = null,
        string? searchName = null,
        string? sortBy = "createdAtDesc",
        CancellationToken ct = default);
    Task<TrainingPlansSearchPagedResponse> SearchPublicTemplates(
        int page = 1,
        int pageSize = 12,
        string? searchName = null,
        TrainingSplitType? splitType = null,
        DifficultyLevel? difficulty = null,
        int? minSessions = null,
        string? sortBy = "popularity",
        CancellationToken ct = default);
    Task<TrainingPlansSearchPagedResponse> GetAssignedToStudent(
        Guid studentId,
        int page = 1,
        int pageSize = 10,
        TrainingPlanStatus? statusFilter = null,
        CancellationToken ct = default);
}
