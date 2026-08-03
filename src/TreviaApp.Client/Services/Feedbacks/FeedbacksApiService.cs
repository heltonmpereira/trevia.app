using System.Net.Http.Json;
using System.Text;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Feedbacks.Requests;
using TreviaApp.Contracts.Feedbacks.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services.Feedbacks;

public class FeedbacksApiService : IFeedbacksService
{
    private readonly HttpClient _http;

    public FeedbacksApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    public async Task<WorkoutFeedbackResponse> CreateWorkoutFeedback(Guid sessionId, CreateWorkoutFeedbackRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/feedbacks/workout-sessions/" + sessionId, request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<WorkoutFeedbackResponse>(ct))!;
    }

    public async Task<ExerciseFeedbackResponse> CreateExerciseFeedback(Guid exerciseId, CreateExerciseFeedbackRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/feedbacks/workout-exercises/" + exerciseId, request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseFeedbackResponse>(ct))!;
    }

    public async Task<SetFeedbackResponse> CreateSetFeedback(Guid setId, CreateSetFeedbackRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/feedbacks/workout-sets/" + setId, request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<SetFeedbackResponse>(ct))!;
    }

    public async Task<UnifiedFeedbackItemResponse> UpdateFeedback(Guid feedbackId, FeedbackLevel level, UpdateFeedbackRequest request, CancellationToken ct = default)
    {
        var qs = QueryString.Build(("level", level.ToString()));
        var resp = await _http.PutAsJsonAsync("api/feedbacks/" + feedbackId + qs.ToString(), request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<UnifiedFeedbackItemResponse>(ct))!;
    }

    public async Task DeleteFeedback(Guid feedbackId, FeedbackLevel level, CancellationToken ct = default)
    {
        var qs = QueryString.Build(("level", level.ToString()));
        var resp = await _http.DeleteAsync("api/feedbacks/" + feedbackId + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<FeedbacksBySessionBundleResponse> GetFeedbacksBySession(Guid sessionId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/feedbacks/workout-sessions/" + sessionId, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<FeedbacksBySessionBundleResponse>(ct))!;
    }

    public async Task MarkFeedbackAsRead(Guid feedbackId, FeedbackLevel level, CancellationToken ct = default)
    {
        var qs = QueryString.Build(("level", level.ToString()));
        var resp = await _http.PutAsync("api/feedbacks/" + feedbackId + "/read" + qs.ToString(), new StringContent("", Encoding.UTF8, "application/json"), ct);
        await EnsureSuccessOrThrow(resp);
    }

    public async Task<PaginatedResponse<UnifiedFeedbackItemResponse>> GetMyFeedbacks(int page = 1, int pageSize = 20, Guid? workoutSessionId = null, bool? onlyUnread = null, FeedbackLevel? level = null, CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (workoutSessionId.HasValue)
            qs = qs.Add(("workoutSessionId", workoutSessionId.Value.ToString()));
        if (onlyUnread.HasValue)
            qs = qs.Add(("onlyUnread", onlyUnread.Value.ToString().ToLowerInvariant()));
        if (level.HasValue)
            qs = qs.Add(("level", level.Value.ToString()));
        var resp = await _http.GetAsync("api/feedbacks/my" + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PaginatedResponse<UnifiedFeedbackItemResponse>>(ct))!;
    }

    public async Task<PaginatedResponse<UnifiedFeedbackItemResponse>> GetStudentFeedbacks(Guid studentId, int page = 1, int pageSize = 20, Guid? workoutSessionId = null, FeedbackLevel? level = null, CancellationToken ct = default)
    {
        var qs = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        if (workoutSessionId.HasValue)
            qs = qs.Add(("workoutSessionId", workoutSessionId.Value.ToString()));
        if (level.HasValue)
            qs = qs.Add(("level", level.Value.ToString()));
        var resp = await _http.GetAsync("api/feedbacks/students/" + studentId + qs.ToString(), ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PaginatedResponse<UnifiedFeedbackItemResponse>>(ct))!;
    }

    public async Task<ExerciseFeedbackResponse> RespondToExerciseFeedback(Guid feedbackId, RespondToExerciseFeedbackRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/feedbacks/exercise-feedbacks/" + feedbackId + "/respond", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ExerciseFeedbackResponse>(ct))!;
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
