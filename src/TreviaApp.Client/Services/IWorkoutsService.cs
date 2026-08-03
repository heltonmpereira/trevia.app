using TreviaApp.Contracts.WorkoutExecution.Requests;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public interface IWorkoutsService
{
    Task<WorkoutSessionSummaryResponse> Start(Guid trainingSessionId, StartWorkoutSessionRequest? request = null, CancellationToken ct = default);
    Task<WorkoutSessionSummaryResponse> Pause(Guid workoutSessionId, CancellationToken ct = default);
    Task<WorkoutSessionSummaryResponse> Resume(Guid workoutSessionId, CancellationToken ct = default);
    Task<WorkoutSessionSummaryResponse> Finish(Guid workoutSessionId, FinishWorkoutSessionRequest? request = null, CancellationToken ct = default);
    Task<WorkoutExerciseResponse> SkipExercise(Guid workoutSessionId, Guid workoutExerciseId, SkipWorkoutExerciseRequest? request = null, CancellationToken ct = default);
    Task<WorkoutSetResponse> AddExtraSet(Guid workoutSessionId, Guid workoutExerciseId, AddExtraSetRequest? request = null, CancellationToken ct = default);
    Task<WorkoutSetResponse> LogWorkoutSet(Guid workoutSessionId, Guid workoutExerciseId, Guid workoutSetId, LogWorkoutSetRequest request, CancellationToken ct = default);
    Task<WorkoutSessionsPagedResponse> GetMy(
        WorkoutStatus? statusFilter = null,
        int page = 1,
        int pageSize = 20,
        Guid? trainingPlanId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default);
    Task<WorkoutSessionsPagedResponse> GetStudentSessions(
        Guid studentId,
        WorkoutStatus? statusFilter = null,
        int page = 1,
        int pageSize = 20,
        Guid? trainingPlanId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default);
    Task<WorkoutSessionDetailResponse?> GetCurrentActive(CancellationToken ct = default);
    Task<WorkoutSessionDetailResponse?> GetStudentCurrentActive(Guid studentId, CancellationToken ct = default);
    Task<WorkoutSessionDetailResponse> GetById(Guid workoutSessionId, CancellationToken ct = default);
    Task<WorkoutSessionDetailResponse> GetStudentSessionById(Guid studentId, Guid workoutSessionId, CancellationToken ct = default);
}
