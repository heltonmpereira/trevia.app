using TreviaApp.Contracts.Gamification.Requests;
using TreviaApp.Contracts.Gamification.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public interface IGamificationService
{
    Task<PointBalanceResponse> GetMyPointsBalance(CancellationToken ct = default);
    Task<PointBalanceResponse> GetStudentPointsBalance(Guid userId, CancellationToken ct = default);
    Task<PointHistoryPagedResponse> GetMyPointsHistory(int page = 1, int pageSize = 20, PointReason? reason = null, CancellationToken ct = default);
    Task<PointHistoryPagedResponse> GetStudentPointsHistory(Guid userId, int page = 1, int pageSize = 20, PointReason? reason = null, CancellationToken ct = default);
    Task<AwardWorkoutPointsResultResponse> AwardWorkoutPoints(Guid sessionId, CancellationToken ct = default);
    Task<int> AdjustPoints(AdjustPointsRequest request, Guid targetUserId, CancellationToken ct = default);

    Task<UserLevelProgressResponse> GetMyLevelProgress(CancellationToken ct = default);
    Task<UserLevelProgressResponse> GetStudentLevelProgress(Guid userId, CancellationToken ct = default);

    Task<AchievementsPagedResponse> GetAllAchievementsWithProgress(int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<AchievementsPagedResponse> GetStudentAchievements(Guid userId, int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<List<AchievementProgressResponse>> GetMyRecentAchievements(int top = 5, CancellationToken ct = default);
    Task<List<AchievementProgressResponse>> GetStudentRecentAchievements(Guid userId, int top = 5, CancellationToken ct = default);

    Task<StreaksSummaryResponse> GetMyStreaks(CancellationToken ct = default);
    Task<StreaksSummaryResponse> GetStudentStreaks(Guid userId, CancellationToken ct = default);
    Task<RecomputeStreaksResultResponse> RecomputeStreaks(CancellationToken ct = default);

    Task<List<UserMissionProgressResponse>> GetMyTodayMissions(DateTime? date = null, CancellationToken ct = default);
    Task<List<UserMissionProgressResponse>> GetStudentTodayMissions(Guid userId, DateTime? date = null, CancellationToken ct = default);
    Task<List<UserMissionProgressResponse>> GetMyThisWeekMissions(DateTime? weekStart = null, CancellationToken ct = default);
    Task<List<UserMissionProgressResponse>> GetStudentThisWeekMissions(Guid userId, DateTime? weekStart = null, CancellationToken ct = default);
    Task<ClaimMissionResultResponse> ClaimMissionReward(Guid missionId, string type = "Daily", DateTime? date = null, CancellationToken ct = default);

    Task<GamificationDashboardResponse> GetMyDashboard(CancellationToken ct = default);
    Task<GamificationDashboardResponse> GetStudentDashboard(Guid studentId, CancellationToken ct = default);
}
