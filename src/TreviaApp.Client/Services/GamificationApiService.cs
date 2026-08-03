using System.Net.Http.Json;
using TreviaApp.Contracts.Gamification.Requests;
using TreviaApp.Contracts.Gamification.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public class GamificationApiService : IGamificationService
{
    private readonly HttpClient _http;

    public GamificationApiService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("TreviaApp.Api");
    }

    #region ===== POINTS =====

    public async Task<PointBalanceResponse> GetMyPointsBalance(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/gamification/points/balance", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PointBalanceResponse>(cancellationToken: ct))!;
    }

    public async Task<PointBalanceResponse> GetStudentPointsBalance(Guid userId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/gamification/points/balance/users/{userId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PointBalanceResponse>(cancellationToken: ct))!;
    }

    public async Task<PointHistoryPagedResponse> GetMyPointsHistory(int page = 1, int pageSize = 20, PointReason? reason = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()),
            ("reason", reason.HasValue ? ((int)reason.Value).ToString() : null));
        var resp = await _http.GetAsync($"api/gamification/points/history{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PointHistoryPagedResponse>(cancellationToken: ct))!;
    }

    public async Task<PointHistoryPagedResponse> GetStudentPointsHistory(Guid userId, int page = 1, int pageSize = 20, PointReason? reason = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()),
            ("reason", reason.HasValue ? ((int)reason.Value).ToString() : null));
        var resp = await _http.GetAsync($"api/gamification/points/history/users/{userId}{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<PointHistoryPagedResponse>(cancellationToken: ct))!;
    }

    public async Task<AwardWorkoutPointsResultResponse> AwardWorkoutPoints(Guid sessionId, CancellationToken ct = default)
    {
        var resp = await _http.PostAsync($"api/gamification/points/award/workout/{sessionId}", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<AwardWorkoutPointsResultResponse>(cancellationToken: ct))!;
    }

    public async Task<int> AdjustPoints(AdjustPointsRequest request, Guid targetUserId, CancellationToken ct = default)
    {
        var query = BuildQueryString(("targetUserId", targetUserId.ToString()));
        var resp = await _http.PostAsJsonAsync($"api/gamification/points/adjust{query}", request, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: ct));
    }

    #endregion

    #region ===== PROGRESS / LEVEL =====

    public async Task<UserLevelProgressResponse> GetMyLevelProgress(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/gamification/progress", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<UserLevelProgressResponse>(cancellationToken: ct))!;
    }

    public async Task<UserLevelProgressResponse> GetStudentLevelProgress(Guid userId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/gamification/progress/users/{userId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<UserLevelProgressResponse>(cancellationToken: ct))!;
    }

    #endregion

    #region ===== ACHIEVEMENTS =====

    public async Task<AchievementsPagedResponse> GetAllAchievementsWithProgress(int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        var resp = await _http.GetAsync($"api/gamification/achievements{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<AchievementsPagedResponse>(cancellationToken: ct))!;
    }

    public async Task<AchievementsPagedResponse> GetStudentAchievements(Guid userId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));
        var resp = await _http.GetAsync($"api/gamification/achievements/users/{userId}{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<AchievementsPagedResponse>(cancellationToken: ct))!;
    }

    public async Task<List<AchievementProgressResponse>> GetMyRecentAchievements(int top = 5, CancellationToken ct = default)
    {
        var query = BuildQueryString(("top", top.ToString()));
        var resp = await _http.GetAsync($"api/gamification/achievements/recent{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<AchievementProgressResponse>>(cancellationToken: ct))!;
    }

    public async Task<List<AchievementProgressResponse>> GetStudentRecentAchievements(Guid userId, int top = 5, CancellationToken ct = default)
    {
        var query = BuildQueryString(("top", top.ToString()));
        var resp = await _http.GetAsync($"api/gamification/achievements/recent/users/{userId}{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<AchievementProgressResponse>>(cancellationToken: ct))!;
    }

    #endregion

    #region ===== STREAKS =====

    public async Task<StreaksSummaryResponse> GetMyStreaks(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/gamification/streaks", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<StreaksSummaryResponse>(cancellationToken: ct))!;
    }

    public async Task<StreaksSummaryResponse> GetStudentStreaks(Guid userId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/gamification/streaks/users/{userId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<StreaksSummaryResponse>(cancellationToken: ct))!;
    }

    public async Task<RecomputeStreaksResultResponse> RecomputeStreaks(CancellationToken ct = default)
    {
        var resp = await _http.PostAsync("api/gamification/streaks/recompute", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<RecomputeStreaksResultResponse>(cancellationToken: ct))!;
    }

    #endregion

    #region ===== MISSIONS =====

    public async Task<List<UserMissionProgressResponse>> GetMyTodayMissions(DateTime? date = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(("date", date.HasValue ? date.Value.ToString("O") : null));
        var resp = await _http.GetAsync($"api/gamification/missions/today{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<UserMissionProgressResponse>>(cancellationToken: ct))!;
    }

    public async Task<List<UserMissionProgressResponse>> GetStudentTodayMissions(Guid userId, DateTime? date = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(("date", date.HasValue ? date.Value.ToString("O") : null));
        var resp = await _http.GetAsync($"api/gamification/missions/today/users/{userId}{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<UserMissionProgressResponse>>(cancellationToken: ct))!;
    }

    public async Task<List<UserMissionProgressResponse>> GetMyThisWeekMissions(DateTime? weekStart = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(("weekStart", weekStart.HasValue ? weekStart.Value.ToString("O") : null));
        var resp = await _http.GetAsync($"api/gamification/missions/this-week{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<UserMissionProgressResponse>>(cancellationToken: ct))!;
    }

    public async Task<List<UserMissionProgressResponse>> GetStudentThisWeekMissions(Guid userId, DateTime? weekStart = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(("weekStart", weekStart.HasValue ? weekStart.Value.ToString("O") : null));
        var resp = await _http.GetAsync($"api/gamification/missions/this-week/users/{userId}{query}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<List<UserMissionProgressResponse>>(cancellationToken: ct))!;
    }

    public async Task<ClaimMissionResultResponse> ClaimMissionReward(Guid missionId, string type = "Daily", DateTime? date = null, CancellationToken ct = default)
    {
        var query = BuildQueryString(
            ("type", type),
            ("date", date.HasValue ? date.Value.ToString("O") : null));
        var resp = await _http.PostAsync($"api/gamification/missions/{missionId}/claim{query}", null, ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<ClaimMissionResultResponse>(cancellationToken: ct))!;
    }

    #endregion

    #region ===== DASHBOARD =====

    public async Task<GamificationDashboardResponse> GetMyDashboard(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/gamification/dashboard", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<GamificationDashboardResponse>(cancellationToken: ct))!;
    }

    public async Task<GamificationDashboardResponse> GetStudentDashboard(Guid studentId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"api/gamification/dashboard/students/{studentId}", ct);
        await EnsureSuccessOrThrow(resp);
        return (await resp.Content.ReadFromJsonAsync<GamificationDashboardResponse>(cancellationToken: ct))!;
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
