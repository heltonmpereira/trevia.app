namespace TreviaApp.Infrastructure.Persistence.Seeder;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Domain.Gamification;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public static class GamificationSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        await SeedAchievementsAsync(db, ct);
        await SeedDailyMissionsAsync(db, ct);
        await SeedWeeklyMissionsAsync(db, ct);
    }

    private static async Task SeedAchievementsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var achievements = new (string Code, string Name, string Description, AchievementCategory Category, int Points, string? Cfg)[]
        {
            (GamificationConstants.AchievementCodes.AC001, "Primeiro Passo", "Concluir o seu primeiro treino", AchievementCategory.Milestone, 100, "{\"WorkoutCount\": 1}"),
            (GamificationConstants.AchievementCodes.AC002, "Frequência 10", "Concluir 10 treinos", AchievementCategory.Milestone, 200, "{\"WorkoutCount\": 10}"),
            (GamificationConstants.AchievementCodes.AC003, "Semana Completa", "Treinar 7 dias consecutivos", AchievementCategory.Streaks, 300, "{\"StreakDays\": 7}"),
            (GamificationConstants.AchievementCodes.AC004, "Mês Integro", "Treinar 30 dias consecutivos", AchievementCategory.Streaks, 1000, "{\"StreakDays\": 30}"),
            (GamificationConstants.AchievementCodes.AC005, "Primeiro Record", "Bater seu primeiro recorde pessoal", AchievementCategory.Performance, 150, "{\"PersonalRecordCount\": 1}"),
            (GamificationConstants.AchievementCodes.AC006, "Leitor de Feedbacks", "Ler 5 feedbacks do professor", AchievementCategory.Social, 100, "{\"FeedbackReadCount\": 5}"),
            (GamificationConstants.AchievementCodes.AC007, "Série Concluída 100", "Concluir 100 séries com sucesso", AchievementCategory.Performance, 250, "{\"SetCount\": 100}"),
            (GamificationConstants.AchievementCodes.AC008, "Ficha Completada", "Concluir uma ficha de treino completa", AchievementCategory.Milestone, 400, "{\"PlanCompleted\": 1}"),
            (GamificationConstants.AchievementCodes.AC009, "Nível 5", "Alcançar o nível 5", AchievementCategory.Milestone, 500, "{\"Level\": 5}"),
            (GamificationConstants.AchievementCodes.AC010, "Nível 10", "Alcançar o nível 10", AchievementCategory.Milestone, 1000, "{\"Level\": 10}"),
        };

        var existing = await db.Set<AchievementDefinition>()
            .AsNoTracking()
            .Select(d => d.Code)
            .ToHashSetAsync(ct);

        foreach (var (code, name, desc, cat, pts, cfg) in achievements)
        {
            if (!existing.Contains(code))
            {
                db.Set<AchievementDefinition>().Add(new AchievementDefinition(
                    code, name, desc, cat, pts, criteriaConfigJson: cfg));
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedDailyMissionsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var missions = new (string Code, string Title, string Description, int Target, MissionMetric Metric, int Points, int Xp)[]
        {
            (GamificationConstants.DailyMissionCodes.D1, "Treino do Dia", "Completar 1 treino hoje", 1, MissionMetric.WorkoutsCompleted, 30, 50),
            (GamificationConstants.DailyMissionCodes.D2, "Dez Séries", "Concluir 10 séries hoje", 10, MissionMetric.SetsCompleted, 20, 30),
            (GamificationConstants.DailyMissionCodes.D3, "Feedback Lido", "Ler 1 feedback do seu professor", 1, MissionMetric.FeedbackRead, 10, 20),
        };

        var existing = await db.Set<DailyMissionDefinition>()
            .AsNoTracking()
            .Select(d => d.Code)
            .ToHashSetAsync(ct);

        foreach (var (code, title, desc, target, metric, points, xp) in missions)
        {
            if (!existing.Contains(code))
            {
                db.Set<DailyMissionDefinition>().Add(new DailyMissionDefinition(
                    code, title, desc, target, metric, points, xp));
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedWeeklyMissionsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var missions = new (string Code, string Title, string Description, int Target, MissionMetric Metric, int Points, int Xp)[]
        {
            (GamificationConstants.WeeklyMissionCodes.W1, "3 Dias de Treino", "Treinar 3 dias nesta semana", 3, MissionMetric.WorkoutsCompleted, 100, 150),
            (GamificationConstants.WeeklyMissionCodes.W2, "40 Séries", "Concluir 40 séries durante a semana", 40, MissionMetric.SetsCompleted, 80, 100),
        };

        var existing = await db.Set<WeeklyMissionDefinition>()
            .AsNoTracking()
            .Select(d => d.Code)
            .ToHashSetAsync(ct);

        foreach (var (code, title, desc, target, metric, points, xp) in missions)
        {
            if (!existing.Contains(code))
            {
                db.Set<WeeklyMissionDefinition>().Add(new WeeklyMissionDefinition(
                    code, title, desc, target, metric, points, xp));
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
