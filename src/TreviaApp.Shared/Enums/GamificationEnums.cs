namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

public enum PointReason
{
    [Description("Treino concluído")]
    WorkoutCompleted = 0,

    [Description("Série concluída")]
    SetCompleted = 1,

    [Description("Feedback lido")]
    ReadFeedback = 2,

    [Description("Streak de 7 dias")]
    Streak7Days = 3,

    [Description("Conquista desbloqueada")]
    AchievementUnlocked = 4,

    [Description("Missão diária concluída")]
    DailyMissionCompleted = 5,

    [Description("Missão semanal concluída")]
    WeeklyMissionCompleted = 6,

    [Description("Ajuste manual")]
    ManualAdjustment = 7,

    [Description("Nível subiu")]
    LevelUp = 8,

    [Description("Streak de 30 dias")]
    Streak30Days = 9
}

public enum AchievementCategory
{
    [Description("Marco")]
    Milestone = 0,

    [Description("Sequências")]
    Streaks = 1,

    [Description("Performance")]
    Performance = 2,

    [Description("Social")]
    Social = 3
}

public enum MissionMetric
{
    [Description("Treinos concluídos")]
    WorkoutsCompleted = 0,

    [Description("Séries concluídas")]
    SetsCompleted = 1,

    [Description("Feedbacks lidos")]
    FeedbackRead = 2,

    [Description("Minutos treinados")]
    MinutesTrained = 3
}

public enum StreakType
{
    [Description("Diário de treino")]
    DailyWorkout = 0,

    [Description("Semanal de treino")]
    WeeklyWorkout = 1
}
