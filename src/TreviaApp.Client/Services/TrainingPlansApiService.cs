using System.Net.Http.Json;
using TreviaApp.Contracts.TrainingPlans.Requests;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public class TrainingPlansApiService : ITrainingPlansService
{
    private readonly HttpClient _http;

    public TrainingPlansApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<TrainingPlanDetailResponse> Create(CreateTrainingPlanRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/trainingplans", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Update(Guid planId, UpdateTrainingPlanRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/trainingplans/{planId}", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task Delete(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/trainingplans/{planId}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<TrainingPlanDetailResponse> Duplicate(Guid planId, DuplicatePlanRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/trainingplans/{planId}/duplicate", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Publish(Guid planId, PublishPlanRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/trainingplans/{planId}/publish", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Unpublish(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.PostAsync($"api/trainingplans/{planId}/unpublish", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> AssignToStudent(Guid planId, Guid studentId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/trainingplans/{planId}/assign/{studentId}", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Pause(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/trainingplans/{planId}/pause", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Resume(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/trainingplans/{planId}/resume", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Complete(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/trainingplans/{planId}/complete", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> Archive(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/trainingplans/{planId}/archive", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> AddSession(Guid planId, AddTrainingSessionRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/trainingplans/{planId}/sessions", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> UpdateSession(Guid planId, Guid sessionId, UpdateTrainingSessionRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/trainingplans/{planId}/sessions/{sessionId}", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> RemoveSession(Guid planId, Guid sessionId, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/trainingplans/{planId}/sessions/{sessionId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> ReorderSessions(Guid planId, ReorderSessionsRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/trainingplans/{planId}/sessions/reorder", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> AddExerciseToSession(Guid planId, Guid sessionId, AddExerciseToSessionRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/trainingplans/{planId}/sessions/{sessionId}/exercises", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> UpdateExerciseInSession(Guid planId, Guid sessionId, Guid sessionExerciseId, UpdateExerciseInSessionRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/trainingplans/{planId}/sessions/{sessionId}/exercises/{sessionExerciseId}", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> RemoveExerciseFromSession(Guid planId, Guid sessionId, Guid sessionExerciseId, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/trainingplans/{planId}/sessions/{sessionId}/exercises/{sessionExerciseId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> ReorderExercisesInSession(Guid planId, Guid sessionId, ReorderExercisesInSessionRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/trainingplans/{planId}/sessions/{sessionId}/exercises/reorder", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> UpsertPrescriptionSets(Guid planId, Guid sessionId, Guid sessionExerciseId, UpsertPrescriptionSetsRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/trainingplans/{planId}/sessions/{sessionId}/exercises/{sessionExerciseId}/prescription-sets", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlanDetailResponse> GetById(Guid planId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/trainingplans/{planId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>(ct))!;
    }

    public async Task<TrainingPlansSearchPagedResponse> GetMyPlans(
        int page = 1,
        int pageSize = 10,
        TrainingPlanStatus? statusFilter = null,
        string? searchName = null,
        string? sortBy = "createdAtDesc",
        CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"page={Uri.EscapeDataString(page.ToString())}",
            $"pageSize={Uri.EscapeDataString(pageSize.ToString())}"
        };
        if (statusFilter.HasValue)
            queryParams.Add($"statusFilter={Uri.EscapeDataString(statusFilter.Value.ToString())}");
        if (!string.IsNullOrEmpty(searchName))
            queryParams.Add($"searchName={Uri.EscapeDataString(searchName)}");
        if (!string.IsNullOrEmpty(sortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        var query = string.Join("&", queryParams);

        var resp = await _http.GetAsync($"api/trainingplans/mine?{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlansSearchPagedResponse>(ct))!;
    }

    public async Task<TrainingPlansSearchPagedResponse> SearchPublicTemplates(
        int page = 1,
        int pageSize = 12,
        string? searchName = null,
        TrainingSplitType? splitType = null,
        DifficultyLevel? difficulty = null,
        int? minSessions = null,
        string? sortBy = "popularity",
        CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"page={Uri.EscapeDataString(page.ToString())}",
            $"pageSize={Uri.EscapeDataString(pageSize.ToString())}"
        };
        if (!string.IsNullOrEmpty(searchName))
            queryParams.Add($"searchName={Uri.EscapeDataString(searchName)}");
        if (splitType.HasValue)
            queryParams.Add($"splitType={Uri.EscapeDataString(splitType.Value.ToString())}");
        if (difficulty.HasValue)
            queryParams.Add($"difficulty={Uri.EscapeDataString(difficulty.Value.ToString())}");
        if (minSessions.HasValue)
            queryParams.Add($"minSessions={Uri.EscapeDataString(minSessions.Value.ToString())}");
        if (!string.IsNullOrEmpty(sortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        var query = string.Join("&", queryParams);

        var resp = await _http.GetAsync($"api/trainingplans?{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlansSearchPagedResponse>(ct))!;
    }

    public async Task<TrainingPlansSearchPagedResponse> GetAssignedToStudent(
        Guid studentId,
        int page = 1,
        int pageSize = 10,
        TrainingPlanStatus? statusFilter = null,
        CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"page={Uri.EscapeDataString(page.ToString())}",
            $"pageSize={Uri.EscapeDataString(pageSize.ToString())}"
        };
        if (statusFilter.HasValue)
            queryParams.Add($"statusFilter={Uri.EscapeDataString(statusFilter.Value.ToString())}");
        var query = string.Join("&", queryParams);

        var resp = await _http.GetAsync($"api/trainingplans/assigned/student/{studentId}?{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<TrainingPlansSearchPagedResponse>(ct))!;
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
