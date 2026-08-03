using System.Net.Http.Json;
using TreviaApp.Contracts.Profiles.Requests;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public class ProfilesApiService : IProfileService
{
    private readonly HttpClient _http;

    public ProfilesApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<ProfileFullResponse> CreateProfile(CreateProfileRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/profiles", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ProfileFullResponse>(ct))!;
    }

    public async Task<ProfileFullResponse> UpdateProfile(UpdateProfileRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync("api/profiles", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ProfileFullResponse>(ct))!;
    }

    public async Task<ProfileFullResponse> GetMyProfile(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/profiles/me", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ProfileFullResponse>(ct))!;
    }

    public async Task<ProfileFullResponse> GetProfileByUserId(Guid userId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/profiles/{userId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ProfileFullResponse>(ct))!;
    }

    public async Task DeleteProfile(CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync("api/profiles", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<WeightEntryResponse> UpsertWeightEntry(UpsertWeightEntryRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/profiles/weight", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WeightEntryResponse>(ct))!;
    }

    public async Task<WeightHistoryResponse> GetWeightHistory(int page = 1, int pageSize = 30, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/profiles/weight?page={page}&pageSize={pageSize}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WeightHistoryResponse>(ct))!;
    }

    public async Task DeleteWeightEntry(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/profiles/weight/{id}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<MeasurementResponse> UpsertMeasurement(UpsertMeasurementRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/profiles/measurements", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<MeasurementResponse>(ct))!;
    }

    public async Task<MeasurementHistoryResponse> GetMeasurementHistory(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/profiles/measurements?page={page}&pageSize={pageSize}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<MeasurementHistoryResponse>(ct))!;
    }

    public async Task DeleteMeasurement(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/profiles/measurements/{id}", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<PhotoUploadResultResponse> UploadProfilePhoto(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var resp = await _http.PostAsync("api/profiles/photo", content, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PhotoUploadResultResponse>(ct))!;
    }

    public async Task RemoveProfilePhoto(CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync("api/profiles/photo", ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<List<Equipment>> UpdateEquipments(UpdateEquipmentsRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync("api/profiles/equipments", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<Equipment>>(ct))!;
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
