using System.Net.Http.Json;
using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public class ExercisesApiService : IExerciseService
{
    private readonly HttpClient _http;

    public ExercisesApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<ExerciseDetailResponse> Create(CreateExerciseRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/exercises", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseDetailResponse>(ct))!;
    }

    public async Task<ExerciseDetailResponse> Update(Guid exerciseId, UpdateExerciseRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/exercises/{exerciseId}", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseDetailResponse>(ct))!;
    }

    public async Task Delete(Guid exerciseId, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/exercises/{exerciseId}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task SubmitForApproval(Guid exerciseId, CancellationToken ct = default)
    {
        var resp = await _http.PostAsync($"api/exercises/{exerciseId}/submit", null, ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task Approve(Guid exerciseId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/exercises/{exerciseId}/approve", null, ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task Reject(Guid exerciseId, RejectExerciseRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/exercises/{exerciseId}/reject", request, ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<ExerciseDetailResponse> GetById(Guid exerciseId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/exercises/{exerciseId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseDetailResponse>(ct))!;
    }

    public async Task<ExerciseSearchPagedResponse> GetMine(int page = 1, int pageSize = 20, ExerciseStatus? status = null, CancellationToken ct = default)
    {
        var url = $"api/exercises/mine?page={page}&pageSize={pageSize}";
        if (status.HasValue)
            url += $"&status={status.Value:D}";

        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseSearchPagedResponse>(ct))!;
    }

    public async Task<ExerciseSearchPagedResponse> SearchApproved(SearchExercisesRequest request, CancellationToken ct = default)
    {
        var qs = BuildSearchQueryString(request);
        var resp = await _http.GetAsync($"api/exercises?{qs}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseSearchPagedResponse>(ct))!;
    }

    public async Task<ExerciseSearchPagedResponse> SearchAll(SearchExercisesRequest filters, bool includeDeleted = false, CancellationToken ct = default)
    {
        var qs = BuildSearchQueryString(filters);
        if (includeDeleted)
            qs += (qs.Length > 0 ? "&" : "") + $"includeDeleted=true";

        var resp = await _http.GetAsync($"api/exercises/all?{qs}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseSearchPagedResponse>(ct))!;
    }

    public async Task<int> GetAwaitingApprovalCount(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/exercises/awaiting-approval/count", ct);
        await EnsureSuccessOrThrow(resp);
        return await resp.Content.ReadFromJsonAsync<int>(ct);
    }

    public async Task<ExerciseMediaResponse> AddMedia(
        Guid exerciseId,
        Stream stream,
        string fileName,
        string contentType,
        int order = 0,
        string? caption = null,
        bool isPrimary = false,
        MediaType? mediaType = null,
        CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();

        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        content.Add(new StringContent(order.ToString()), "order");
        if (!string.IsNullOrEmpty(caption))
            content.Add(new StringContent(caption), "caption");
        content.Add(new StringContent(isPrimary.ToString().ToLowerInvariant()), "isPrimary");
        if (mediaType.HasValue)
            content.Add(new StringContent(mediaType.Value.ToString()), "mediaType");

        var resp = await _http.PostAsync($"api/exercises/{exerciseId}/media", content, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseMediaResponse>(ct))!;
    }

    public async Task RemoveMedia(Guid exerciseId, Guid mediaId, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/exercises/{exerciseId}/media/{mediaId}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task SetPrimaryMedia(Guid exerciseId, Guid mediaId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/exercises/{exerciseId}/media/{mediaId}/primary", null, ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<ExerciseMuscleResponse> AddMuscle(Guid exerciseId, AddMuscleToExerciseRequest dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/exercises/{exerciseId}/muscles", dto, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseMuscleResponse>(ct))!;
    }

    public async Task RemoveMuscle(Guid exerciseId, int muscle, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/exercises/{exerciseId}/muscles/{muscle}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<ExerciseEquipmentResponse> AddEquipment(Guid exerciseId, AddEquipmentToExerciseRequest dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"api/exercises/{exerciseId}/equipments", dto, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseEquipmentResponse>(ct))!;
    }

    public async Task RemoveEquipment(Guid exerciseId, int equipment, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/exercises/{exerciseId}/equipments/{equipment}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    private static string BuildSearchQueryString(SearchExercisesRequest request)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(request.Name))
            parts.Add($"name={Uri.EscapeDataString(request.Name)}");
        if (request.Environment.HasValue)
            parts.Add($"environment={request.Environment.Value:D}");
        if (request.Modality.HasValue)
            parts.Add($"modality={request.Modality.Value:D}");
        if (request.DifficultyLevel.HasValue)
            parts.Add($"difficultyLevel={request.DifficultyLevel.Value:D}");
        if (request.PrimaryMuscle.HasValue)
            parts.Add($"primaryMuscle={request.PrimaryMuscle.Value:D}");
        if (request.Equipment.HasValue)
            parts.Add($"equipment={request.Equipment.Value:D}");
        if (request.MeasurementType.HasValue)
            parts.Add($"measurementType={request.MeasurementType.Value:D}");
        parts.Add($"page={request.Page}");
        parts.Add($"pageSize={request.PageSize}");
        if (!string.IsNullOrEmpty(request.SortBy))
            parts.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");
        parts.Add($"sortDescending={request.SortDescending.ToString().ToLowerInvariant()}");

        return string.Join("&", parts);
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
