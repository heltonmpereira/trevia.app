using TreviaApp.Contracts.Reports.Responses;

namespace TreviaApp.Client.Services;

public interface IReportsService
{
    Task<WorkoutSummaryResponse> GetMySummary(DateTimeOffset? from = null, DateTimeOffset? to = null, Guid? trainingPlanId = null, CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutCalendarDayResponse>> GetMyCalendar(int? year = null, int? month = null, CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutProgressPointResponse>> GetMyProgress(DateTimeOffset? from = null, DateTimeOffset? to = null, ProgressGranularity granularity = ProgressGranularity.Week, CancellationToken ct = default);
    Task<IReadOnlyList<MuscleVolumeItemResponse>> GetMyMuscleDistribution(DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);
    Task<IReadOnlyList<ExerciseRankItemResponse>> GetMyTopExercises(DateTimeOffset? from = null, DateTimeOffset? to = null, int top = 10, ExerciseRankBy rankBy = ExerciseRankBy.Volume, CancellationToken ct = default);
    Task<IReadOnlyList<PersonalRecordItemResponse>> GetMyRecords(Guid? exerciseId = null, CancellationToken ct = default);

    Task<WorkoutSummaryResponse> GetStudentSummary(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, Guid? trainingPlanId = null, CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutCalendarDayResponse>> GetStudentCalendar(Guid studentId, int? year = null, int? month = null, CancellationToken ct = default);
    Task<IReadOnlyList<WorkoutProgressPointResponse>> GetStudentProgress(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, ProgressGranularity granularity = ProgressGranularity.Week, CancellationToken ct = default);
    Task<IReadOnlyList<MuscleVolumeItemResponse>> GetStudentMuscleDistribution(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);
    Task<IReadOnlyList<ExerciseRankItemResponse>> GetStudentTopExercises(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, int top = 10, ExerciseRankBy rankBy = ExerciseRankBy.Volume, CancellationToken ct = default);
    Task<IReadOnlyList<PersonalRecordItemResponse>> GetStudentRecords(Guid studentId, Guid? exerciseId = null, CancellationToken ct = default);
}
