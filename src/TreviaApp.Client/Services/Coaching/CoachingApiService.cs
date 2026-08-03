using System.Net.Http.Json;
using System.Text;
using TreviaApp.Contracts.Coaching.Requests;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services.Coaching;

public class CoachingApiService : ICoachingService
{
    private readonly HttpClient _http;

    public CoachingApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<CoachInviteResponse> SendCoachInvite(SendCoachInviteRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/coaching/invites/send-trainer", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachInviteResponse>(ct))!;
    }

    public async Task<CoachInviteResponse> SendStudentRequest(SendStudentRequestRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/coaching/requests/send-student", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachInviteResponse>(ct))!;
    }

    public async Task<CoachStudentLinkResponse> AcceptInvite(Guid inviteId, AcceptCoachInviteRequest? request = null, CancellationToken ct = default)
    {
        HttpContent content = request is null
            ? new StringContent("{}", Encoding.UTF8, "application/json")
            : JsonContent.Create(request);
        var resp = await _http.PostAsync("api/coaching/invites/" + inviteId + "/accept", content, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentLinkResponse>(ct))!;
    }

    public async Task<CoachInviteResponse> RejectInvite(Guid inviteId, RejectCoachInviteRequest? request = null, CancellationToken ct = default)
    {
        HttpContent content = request is null
            ? new StringContent("{}", Encoding.UTF8, "application/json")
            : JsonContent.Create(request);
        var resp = await _http.PostAsync("api/coaching/invites/" + inviteId + "/reject", content, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachInviteResponse>(ct))!;
    }

    public async Task<CoachInviteResponse> CancelInvite(Guid inviteId, CancellationToken ct = default)
    {
        var resp = await _http.PostAsync("api/coaching/invites/" + inviteId + "/cancel", new StringContent("", Encoding.UTF8, "application/json"), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachInviteResponse>(ct))!;
    }

    public async Task<CoachStudentLinkResponse> EndRelationship(Guid linkId, CoachRelationshipEndReason reason, string? notes = null, CancellationToken ct = default)
    {
        var qs = QueryString.Build(("reason", reason.ToString()));
        if (!string.IsNullOrWhiteSpace(notes))
            qs = qs.Add(("notes", notes!));
        var resp = await _http.DeleteAsync("api/coaching/links/" + linkId + "/end" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentLinkResponse>(ct))!;
    }

    public async Task<CoachStudentLinkResponse> UpdatePermissions(Guid linkId, UpdateCoachPermissionsRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync("api/coaching/links/" + linkId + "/permissions", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentLinkResponse>(ct))!;
    }

    public async Task<CoachingInvitesPagedResponse> GetIncomingInvites(int page = 1, int pageSize = 10, CoachRequestStatus? statusFilter = null, string? sortBy = "createdAtDesc", CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (statusFilter.HasValue)
            qs = qs.Add(("statusFilter", statusFilter.Value.ToString()));
        if (!string.IsNullOrWhiteSpace(sortBy))
            qs = qs.Add(("sortBy", sortBy!));
        var resp = await _http.GetAsync("api/coaching/invites/incoming" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachingInvitesPagedResponse>(ct))!;
    }

    public async Task<CoachingInvitesPagedResponse> GetOutgoingInvites(int page = 1, int pageSize = 10, CoachRequestStatus? statusFilter = null, string? sortBy = "createdAtDesc", CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (statusFilter.HasValue)
            qs = qs.Add(("statusFilter", statusFilter.Value.ToString()));
        if (!string.IsNullOrWhiteSpace(sortBy))
            qs = qs.Add(("sortBy", sortBy!));
        var resp = await _http.GetAsync("api/coaching/invites/outgoing" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachingInvitesPagedResponse>(ct))!;
    }

    public async Task<int> GetPendingInvitesCount(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/coaching/invites/pending-count", ct);
        await EnsureSuccessOrThrow(resp);
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (doc.RootElement.TryGetProperty("pendingCount", out var pc) && pc.TryGetInt32(out var v))
            return v;
        if (doc.RootElement.TryGetProperty("PendingCount", out var pc2) && pc2.TryGetInt32(out var v2))
            return v2;
        return 0;
    }

    public async Task<CoachStudentsPagedResponse> GetMyStudents(int page = 1, int pageSize = 10, string? searchName = null, bool? onlyActive = true, string? sortBy = "linkedSinceDesc", CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (!string.IsNullOrWhiteSpace(searchName))
            qs = qs.Add(("searchName", searchName!));
        if (onlyActive.HasValue)
            qs = qs.Add(("onlyActive", onlyActive.Value.ToString().ToLowerInvariant()));
        if (!string.IsNullOrWhiteSpace(sortBy))
            qs = qs.Add(("sortBy", sortBy!));
        var resp = await _http.GetAsync("api/coaching/students" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentsPagedResponse>(ct))!;
    }

    public async Task<CoachStudentsPagedResponse> GetMyCoaches(int page = 1, int pageSize = 10, string? searchName = null, bool? onlyActive = true, string? sortBy = "linkedSinceDesc", CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (!string.IsNullOrWhiteSpace(searchName))
            qs = qs.Add(("searchName", searchName!));
        if (onlyActive.HasValue)
            qs = qs.Add(("onlyActive", onlyActive.Value.ToString().ToLowerInvariant()));
        if (!string.IsNullOrWhiteSpace(sortBy))
            qs = qs.Add(("sortBy", sortBy!));
        var resp = await _http.GetAsync("api/coaching/coaches" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentsPagedResponse>(ct))!;
    }

    public async Task<CoachStudentLinkResponse> GetRelationshipById(Guid linkId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/coaching/links/" + linkId, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentLinkResponse>(ct))!;
    }

    public async Task<CoachLinkStatusResponse> CheckLinkStatus(Guid otherUserId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/coaching/link-status/" + otherUserId, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachLinkStatusResponse>(ct))!;
    }

    public async Task<CoachStudentsPagedResponse> GetCoachStudentsAsAdmin(Guid coachId, int page = 1, int pageSize = 10, string? searchName = null, bool onlyActive = true, CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()),
            ("onlyActive", onlyActive.ToString().ToLowerInvariant()));
        if (!string.IsNullOrWhiteSpace(searchName))
            qs = qs.Add(("searchName", searchName!));
        var resp = await _http.GetAsync("api/coaching/admin/coaches/" + coachId + "/students" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentsPagedResponse>(ct))!;
    }

    public async Task<CoachStudentsPagedResponse> SearchStudentsNotLinked(string? searchName = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (!string.IsNullOrWhiteSpace(searchName))
            qs = qs.Add(("searchName", searchName!));
        var resp = await _http.GetAsync("api/coaching/search/students" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentsPagedResponse>(ct))!;
    }

    public async Task<CoachStudentsPagedResponse> SearchCoachesNotLinked(string? searchName = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (!string.IsNullOrWhiteSpace(searchName))
            qs = qs.Add(("searchName", searchName!));
        var resp = await _http.GetAsync("api/coaching/search/coaches" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<CoachStudentsPagedResponse>(ct))!;
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

    private ref struct QueryString
    {
        private StringBuilder? _sb;
        public static QueryString Build(params (string Key, string Value)[] items)
        {
            var qs = new QueryString();
            foreach (var (k, v) in items)
                qs = qs.Add((k, v));
            return qs;
        }
        public QueryString Add((string Key, string Value) item)
        {
            _sb ??= new StringBuilder();
            _sb.Append(_sb.Length == 0 ? '?' : '&');
            _sb.Append(Uri.EscapeDataString(item.Key));
            _sb.Append('=');
            _sb.Append(Uri.EscapeDataString(item.Value));
            return this;
        }
        public override string ToString() => _sb?.ToString() ?? "";
    }
}
