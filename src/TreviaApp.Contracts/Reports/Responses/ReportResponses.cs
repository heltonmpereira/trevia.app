using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Reports.Responses;

public sealed record WorkoutSummaryResponse
{
    public WorkoutSummaryResponse() { }

    public WorkoutSummaryResponse(
        DateTimeOffset from,
        DateTimeOffset to,
        int totalWorkouts,
        int completedWorkouts,
        int totalSets,
        int completedSets,
        decimal completionRatePercent,
        decimal? totalVolumeKg,
        long totalDurationSeconds,
        long totalActiveTimeSeconds,
        long? averageWorkoutDurationSeconds,
        long? averageActiveTimeSeconds,
        decimal? averageVolumePerWorkoutKg,
        int uniqueExercisesPerformed,
        decimal? totalDistanceKm,
        int? totalCalories,
        int currentStreakDays,
        int longestStreakDays)
    {
        From = from;
        To = to;
        TotalWorkouts = totalWorkouts;
        CompletedWorkouts = completedWorkouts;
        TotalSets = totalSets;
        CompletedSets = completedSets;
        CompletionRatePercent = completionRatePercent;
        TotalVolumeKg = totalVolumeKg;
        TotalDurationSeconds = totalDurationSeconds;
        TotalActiveTimeSeconds = totalActiveTimeSeconds;
        AverageWorkoutDurationSeconds = averageWorkoutDurationSeconds;
        AverageActiveTimeSeconds = averageActiveTimeSeconds;
        AverageVolumePerWorkoutKg = averageVolumePerWorkoutKg;
        UniqueExercisesPerformed = uniqueExercisesPerformed;
        TotalDistanceKm = totalDistanceKm;
        TotalCalories = totalCalories;
        CurrentStreakDays = currentStreakDays;
        LongestStreakDays = longestStreakDays;
    }

    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
    public int TotalWorkouts { get; init; }
    public int CompletedWorkouts { get; init; }
    public int TotalSets { get; init; }
    public int CompletedSets { get; init; }
    public decimal CompletionRatePercent { get; init; }
    public decimal? TotalVolumeKg { get; init; }
    public long TotalDurationSeconds { get; init; }
    public long TotalActiveTimeSeconds { get; init; }
    public long? AverageWorkoutDurationSeconds { get; init; }
    public long? AverageActiveTimeSeconds { get; init; }
    public decimal? AverageVolumePerWorkoutKg { get; init; }
    public int UniqueExercisesPerformed { get; init; }
    public decimal? TotalDistanceKm { get; init; }
    public int? TotalCalories { get; init; }
    public int CurrentStreakDays { get; init; }
    public int LongestStreakDays { get; init; }
}

public sealed record WorkoutCalendarDayResponse
{
    public WorkoutCalendarDayResponse() { }

    public WorkoutCalendarDayResponse(DateOnly date, int workoutsCount, decimal? totalVolumeKg, long? activeTimeSeconds)
    {
        Date = date;
        WorkoutsCount = workoutsCount;
        TotalVolumeKg = totalVolumeKg;
        ActiveTimeSeconds = activeTimeSeconds;
    }

    public DateOnly Date { get; init; }
    public int WorkoutsCount { get; init; }
    public decimal? TotalVolumeKg { get; init; }
    public long? ActiveTimeSeconds { get; init; }
}

public enum ProgressGranularity
{
    Day = 0,
    Week = 1,
    Month = 2
}

public sealed record WorkoutProgressPointResponse
{
    public WorkoutProgressPointResponse() { }

    public WorkoutProgressPointResponse(
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        int workoutsCount,
        int completedSetsCount,
        decimal? totalVolumeKg,
        long totalDurationSeconds,
        long totalActiveTimeSeconds,
        decimal? totalDistanceKm,
        int? totalCalories)
    {
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        WorkoutsCount = workoutsCount;
        CompletedSetsCount = completedSetsCount;
        TotalVolumeKg = totalVolumeKg;
        TotalDurationSeconds = totalDurationSeconds;
        TotalActiveTimeSeconds = totalActiveTimeSeconds;
        TotalDistanceKm = totalDistanceKm;
        TotalCalories = totalCalories;
    }

    public DateTimeOffset PeriodStart { get; init; }
    public DateTimeOffset PeriodEnd { get; init; }
    public int WorkoutsCount { get; init; }
    public int CompletedSetsCount { get; init; }
    public decimal? TotalVolumeKg { get; init; }
    public long TotalDurationSeconds { get; init; }
    public long TotalActiveTimeSeconds { get; init; }
    public decimal? TotalDistanceKm { get; init; }
    public int? TotalCalories { get; init; }
}

public sealed record MuscleVolumeItemResponse
{
    public MuscleVolumeItemResponse() { }

    public MuscleVolumeItemResponse(Muscle muscle, MuscleRole? muscleRole, decimal totalVolumeKg, int setsCount, decimal percentageOfTotal)
    {
        Muscle = muscle;
        MuscleRole = muscleRole;
        TotalVolumeKg = totalVolumeKg;
        SetsCount = setsCount;
        PercentageOfTotal = percentageOfTotal;
    }

    public Muscle Muscle { get; init; }
    public MuscleRole? MuscleRole { get; init; }
    public decimal TotalVolumeKg { get; init; }
    public int SetsCount { get; init; }
    public decimal PercentageOfTotal { get; init; }
}

public enum ExerciseRankBy
{
    Volume = 0,
    Frequency = 1,
    Sets = 2
}

public sealed record ExerciseRankItemResponse
{
    public ExerciseRankItemResponse() { }

    public ExerciseRankItemResponse(
        Guid exerciseId,
        string exerciseName,
        int rank,
        int workoutsCount,
        int setsCount,
        int completedSetsCount,
        decimal? totalVolumeKg,
        long? totalDurationSeconds,
        decimal? totalDistanceKm)
    {
        ExerciseId = exerciseId;
        ExerciseName = exerciseName;
        Rank = rank;
        WorkoutsCount = workoutsCount;
        SetsCount = setsCount;
        CompletedSetsCount = completedSetsCount;
        TotalVolumeKg = totalVolumeKg;
        TotalDurationSeconds = totalDurationSeconds;
        TotalDistanceKm = totalDistanceKm;
    }

    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public int Rank { get; init; }
    public int WorkoutsCount { get; init; }
    public int SetsCount { get; init; }
    public int CompletedSetsCount { get; init; }
    public decimal? TotalVolumeKg { get; init; }
    public long? TotalDurationSeconds { get; init; }
    public decimal? TotalDistanceKm { get; init; }
}

public enum PersonalRecordType
{
    MaxLoad = 0,
    MaxVolume = 1,
    MaxReps = 2,
    MaxDistance = 3,
    MaxDuration = 4
}

public sealed record PersonalRecordItemResponse
{
    public PersonalRecordItemResponse() { }

    public PersonalRecordItemResponse(
        Guid exerciseId,
        string exerciseName,
        PersonalRecordType recordType,
        decimal value,
        string? unit,
        int? reps,
        DateTimeOffset achievedAt,
        Guid workoutSessionId,
        Guid workoutSetId)
    {
        ExerciseId = exerciseId;
        ExerciseName = exerciseName;
        RecordType = recordType;
        Value = value;
        Unit = unit;
        Reps = reps;
        AchievedAt = achievedAt;
        WorkoutSessionId = workoutSessionId;
        WorkoutSetId = workoutSetId;
    }

    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public PersonalRecordType RecordType { get; init; }
    public decimal Value { get; init; }
    public string? Unit { get; init; }
    public int? Reps { get; init; }
    public DateTimeOffset AchievedAt { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public Guid WorkoutSetId { get; init; }
}
