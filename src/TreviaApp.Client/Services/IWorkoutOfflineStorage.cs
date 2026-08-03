namespace TreviaApp.Client.Services;

public interface IWorkoutOfflineStorage
{
    Task SaveCurrentWorkoutAsync(string userId, WorkoutInProgressData data);
    Task<WorkoutInProgressData?> LoadCurrentWorkoutAsync(string userId);
    Task ClearCurrentWorkoutAsync(string userId);
    Task<bool> HasSavedWorkoutAsync(string userId);
}

public class WorkoutInProgressData
{
    public Guid SessionId { get; set; }
    public string StartedAt { get; set; } = DateTimeOffset.UtcNow.ToString("o");
    public string? PausedAt { get; set; }
    public long ElapsedSeconds { get; set; }
    public int CurrentExerciseIndex { get; set; }
    public List<WorkoutExerciseOfflineData> Exercises { get; set; } = new();
    public string? Notes { get; set; }
}

public class WorkoutExerciseOfflineData
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public List<WorkoutSetOfflineData> Sets { get; set; } = new();
    public int CompletedSets { get; set; }
    public int TotalSets { get; set; }
}

public class WorkoutSetOfflineData
{
    public int SetNumber { get; set; }
    public decimal Load { get; set; }
    public int RepsCompleted { get; set; }
    public int RepsPrescribed { get; set; }
    public bool IsCompleted { get; set; }
    public string? Rating { get; set; }
    public string? Notes { get; set; }
}
