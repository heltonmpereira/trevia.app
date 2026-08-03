using System.Net.Http.Json;
using TreviaApp.Contracts.WorkoutExecution.Requests;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public class WorkoutsApiService : IWorkoutsService
{
    private readonly HttpClient _http;

    public WorkoutsApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<WorkoutSessionSummaryResponse> Start(Guid trainingSessionId, StartWorkoutSessionRequest? request = null, CancellationToken ct = default)
    {
        var body = request ?? new StartWorkoutSessionRequest(trainingSessionId);
        var resp = await _http.PostAsJsonAsync($"api/workouts/sessions/start/{trainingSessionId}", body, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionSummaryResponse>(ct))!;
    }

    public async Task<WorkoutSessionSummaryResponse> Pause(Guid workoutSessionId, CancellationToken ct = default)
    {
        var resp = await _http.PostAsync($"api/workouts/sessions/{workoutSessionId}/pause", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionSummaryResponse>(ct))!;
    }

    public async Task<WorkoutSessionSummaryResponse> Resume(Guid workoutSessionId, CancellationToken ct = default)
    {
        var resp = await _http.PostAsync($"api/workouts/sessions/{workoutSessionId}/resume", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionSummaryResponse>(ct))!;
    }

    public async Task<WorkoutSessionSummaryResponse> Finish(Guid workoutSessionId, FinishWorkoutSessionRequest? request = null, CancellationToken ct = default)
    {
        var body = request ?? new FinishWorkoutSessionRequest();
        var resp = await _http.PostAsJsonAsync($"api/workouts/sessions/{workoutSessionId}/finish", body, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionSummaryResponse>(ct))!;
    }

    public async Task<WorkoutExerciseResponse> SkipExercise(Guid workoutSessionId, Guid workoutExerciseId, SkipWorkoutExerciseRequest? request = null, CancellationToken ct = default)
    {
        var body = request ?? new SkipWorkoutExerciseRequest();
        var resp = await _http.PostAsJsonAsync($"api/workouts/sessions/{workoutSessionId}/exercises/{workoutExerciseId}/skip", body, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutExerciseResponse>(ct))!;
    }

    public async Task<WorkoutSetResponse> AddExtraSet(Guid workoutSessionId, Guid workoutExerciseId, AddExtraSetRequest? request = null, CancellationToken ct = default)
    {
        var body = request ?? new AddExtraSetRequest();
        var resp = await _http.PostAsJsonAsync($"api/workouts/sessions/{workoutSessionId}/exercises/{workoutExerciseId}/extra-set", body, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSetResponse>(ct))!;
    }

    public async Task<WorkoutSetResponse> LogWorkoutSet(Guid workoutSessionId, Guid workoutExerciseId, Guid workoutSetId, LogWorkoutSetRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/workouts/sessions/{workoutSessionId}/exercises/{workoutExerciseId}/sets/{workoutSetId}", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSetResponse>(ct))!;
    }

    public async Task<WorkoutSessionsPagedResponse> GetMy(
        WorkoutStatus? statusFilter = null,
        int page = 1,
        int pageSize = 20,
        Guid? trainingPlanId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"page={Uri.EscapeDataString(page.ToString())}",
            $"pageSize={Uri.EscapeDataString(pageSize.ToString())}"
        };
        if (statusFilter.HasValue)
            queryParams.Add($"statusFilter={Uri.EscapeDataString(statusFilter.Value.ToString())}");
        if (trainingPlanId.HasValue)
            queryParams.Add($"trainingPlanId={Uri.EscapeDataString(trainingPlanId.Value.ToString())}");
        if (from.HasValue)
            queryParams.Add($"from={Uri.EscapeDataString(from.Value.ToString("O"))}");
        if (to.HasValue)
            queryParams.Add($"to={Uri.EscapeDataString(to.Value.ToString("O"))}");
        var query = string.Join("&", queryParams);

        var resp = await _http.GetAsync($"api/workouts/sessions?{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionsPagedResponse>(ct))!;
    }

    public async Task<WorkoutSessionsPagedResponse> GetStudentSessions(
        Guid studentId,
        WorkoutStatus? statusFilter = null,
        int page = 1,
        int pageSize = 20,
        Guid? trainingPlanId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"page={Uri.EscapeDataString(page.ToString())}",
            $"pageSize={Uri.EscapeDataString(pageSize.ToString())}"
        };
        if (statusFilter.HasValue)
            queryParams.Add($"statusFilter={Uri.EscapeDataString(statusFilter.Value.ToString())}");
        if (trainingPlanId.HasValue)
            queryParams.Add($"trainingPlanId={Uri.EscapeDataString(trainingPlanId.Value.ToString())}");
        if (from.HasValue)
            queryParams.Add($"from={Uri.EscapeDataString(from.Value.ToString("O"))}");
        if (to.HasValue)
            queryParams.Add($"to={Uri.EscapeDataString(to.Value.ToString("O"))}");
        var query = string.Join("&", queryParams);

        var resp = await _http.GetAsync($"api/workouts/students/{studentId}/sessions?{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionsPagedResponse>(ct))!;
    }

    public async Task<WorkoutSessionDetailResponse?> GetCurrentActive(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/workouts/sessions/current-active", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        await EnsureSuccessOrThrow(resp);
        return await resp.Content.ReadFromJsonAsync<WorkoutSessionDetailResponse>(ct);
    }

    public async Task<WorkoutSessionDetailResponse?> GetStudentCurrentActive(Guid studentId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/workouts/students/{studentId}/sessions/current-active", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        await EnsureSuccessOrThrow(resp);
        return await resp.Content.ReadFromJsonAsync<WorkoutSessionDetailResponse>(ct);
    }

    public async Task<WorkoutSessionDetailResponse> GetById(Guid workoutSessionId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/workouts/sessions/{workoutSessionId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionDetailResponse>(ct))!;
    }

    public async Task<WorkoutSessionDetailResponse> GetStudentSessionById(Guid studentId, Guid workoutSessionId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/workouts/students/{studentId}/sessions/{workoutSessionId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutSessionDetailResponse>(ct))!;
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
