using System.Net.Http.Json;
using TreviaApp.Contracts.Reports.Responses;

namespace TreviaApp.Client.Services;

public class ReportsApiService : IReportsService
{
    private readonly HttpClient _http;

    public ReportsApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    #region ===== My Reports =====

    public async Task<WorkoutSummaryResponse> GetMySummary(DateTimeOffset? from = null, DateTimeOffset? to = null, Guid? trainingPlanId = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null),
            ("trainingPlanId", trainingPlanId.HasValue ? trainingPlanId.Value.ToString() : null));
        var resp = await _http.GetAsync($"api/reports/summary{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSummaryResponse>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<WorkoutCalendarDayResponse>> GetMyCalendar(int? year = null, int? month = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("year", year.HasValue ? year.Value.ToString() : null),
            ("month", month.HasValue ? month.Value.ToString() : null));
        var resp = await _http.GetAsync($"api/reports/calendar{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<WorkoutCalendarDayResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<WorkoutProgressPointResponse>> GetMyProgress(DateTimeOffset? from = null, DateTimeOffset? to = null, ProgressGranularity granularity = ProgressGranularity.Week, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null),
            ("granularity", ((int)granularity).ToString()));
        var resp = await _http.GetAsync($"api/reports/progress{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<WorkoutProgressPointResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<MuscleVolumeItemResponse>> GetMyMuscleDistribution(DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null));
        var resp = await _http.GetAsync($"api/reports/muscles{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<MuscleVolumeItemResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<ExerciseRankItemResponse>> GetMyTopExercises(DateTimeOffset? from = null, DateTimeOffset? to = null, int top = 10, ExerciseRankBy rankBy = ExerciseRankBy.Volume, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null),
            ("top", top.ToString()),
            ("rankBy", ((int)rankBy).ToString()));
        var resp = await _http.GetAsync($"api/reports/exercises/top{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseRankItemResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<PersonalRecordItemResponse>> GetMyRecords(Guid? exerciseId = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(("exerciseId", exerciseId.HasValue ? exerciseId.Value.ToString() : null));
        var resp = await _http.GetAsync($"api/reports/records{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<PersonalRecordItemResponse>>(cancellationToken: ct))!;
    }

    #endregion

    #region ===== Student Reports =====

    public async Task<WorkoutSummaryResponse> GetStudentSummary(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, Guid? trainingPlanId = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null),
            ("trainingPlanId", trainingPlanId.HasValue ? trainingPlanId.Value.ToString() : null));
        var resp = await _http.GetAsync($"api/reports/students/{studentId}/summary{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSummaryResponse>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<WorkoutCalendarDayResponse>> GetStudentCalendar(Guid studentId, int? year = null, int? month = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("year", year.HasValue ? year.Value.ToString() : null),
            ("month", month.HasValue ? month.Value.ToString() : null));
        var resp = await _http.GetAsync($"api/reports/students/{studentId}/calendar{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<WorkoutCalendarDayResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<WorkoutProgressPointResponse>> GetStudentProgress(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, ProgressGranularity granularity = ProgressGranularity.Week, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null),
            ("granularity", ((int)granularity).ToString()));
        var resp = await _http.GetAsync($"api/reports/students/{studentId}/progress{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<WorkoutProgressPointResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<MuscleVolumeItemResponse>> GetStudentMuscleDistribution(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null));
        var resp = await _http.GetAsync($"api/reports/students/{studentId}/muscles{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<MuscleVolumeItemResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<ExerciseRankItemResponse>> GetStudentTopExercises(Guid studentId, DateTimeOffset? from = null, DateTimeOffset? to = null, int top = 10, ExerciseRankBy rankBy = ExerciseRankBy.Volume, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("from", from.HasValue ? from.Value.ToString("O") : null),
            ("to", to.HasValue ? to.Value.ToString("O") : null),
            ("top", top.ToString()),
            ("rankBy", ((int)rankBy).ToString()));
        var resp = await _http.GetAsync($"api/reports/students/{studentId}/exercises/top{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseRankItemResponse>>(cancellationToken: ct))!;
    }

    public async Task<IReadOnlyList<PersonalRecordItemResponse>> GetStudentRecords(Guid studentId, Guid? exerciseId = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(("exerciseId", exerciseId.HasValue ? exerciseId.Value.ToString() : null));
        var resp = await _http.GetAsync($"api/reports/students/{studentId}/records{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<IReadOnlyList<PersonalRecordItemResponse>>(cancellationToken: ct))!;
    }

    #endregion

    private static string BuildQueryString(params (string Key, string? Value)[] parameters)
    {
        var parts = new List<string>();
        foreach (var (key, value) in parameters)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }
        }
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private static async Task EnsureSuccessOrThrow(HttpResponseMessage resp)
    {
        if (!resp.IsSuccessStatusCode)
        {
            var content = "";
            try { content = await resp.Content.ReadAsStringAsync(); } catch { }
            throw new ApplicationException($"Falha na chamada API ({(int)resp.StatusCode}): {resp.ReasonPhrase}\n{content}");
        }
    }
}
