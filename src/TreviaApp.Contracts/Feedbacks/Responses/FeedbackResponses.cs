using TreviaApp.Contracts.Common;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Contracts.Feedbacks.Responses;

public sealed record WorkoutFeedbackResponse
{
    public WorkoutFeedbackResponse() { }

    public WorkoutFeedbackResponse(
        Guid id,
        Guid coachId,
        string? coachName,
        Guid studentId,
        Guid workoutSessionId,
        string text,
        FeedbackTone tone,
        bool isPublic,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? readAt)
    {
        Id = id;
        CoachId = coachId;
        CoachName = coachName;
        StudentId = studentId;
        WorkoutSessionId = workoutSessionId;
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ReadAt = readAt;
    }

    public FeedbackLevel Level => FeedbackLevel.Session;
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string? CoachName { get; init; }
    public Guid StudentId { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; }
    public bool IsPublic { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}

public sealed record ExerciseFeedbackResponse
{
    public ExerciseFeedbackResponse() { }

    public ExerciseFeedbackResponse(
        Guid id,
        Guid coachId,
        string? coachName,
        Guid studentId,
        Guid workoutSessionId,
        Guid workoutExerciseId,
        string? exerciseName,
        string text,
        FeedbackTone tone,
        bool isPublic,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? readAt,
        string? studentResponseText,
        DateTimeOffset? studentRespondedAt)
    {
        Id = id;
        CoachId = coachId;
        CoachName = coachName;
        StudentId = studentId;
        WorkoutSessionId = workoutSessionId;
        WorkoutExerciseId = workoutExerciseId;
        ExerciseName = exerciseName;
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ReadAt = readAt;
        StudentResponseText = studentResponseText;
        StudentRespondedAt = studentRespondedAt;
    }

    public FeedbackLevel Level => FeedbackLevel.Exercise;
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string? CoachName { get; init; }
    public Guid StudentId { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public Guid WorkoutExerciseId { get; init; }
    public string? ExerciseName { get; init; }
    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; }
    public bool IsPublic { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public string? StudentResponseText { get; init; }
    public DateTimeOffset? StudentRespondedAt { get; init; }
}

public sealed record SetFeedbackResponse
{
    public SetFeedbackResponse() { }

    public SetFeedbackResponse(
        Guid id,
        Guid coachId,
        string? coachName,
        Guid studentId,
        Guid workoutSessionId,
        Guid workoutExerciseId,
        Guid workoutSetId,
        string? exerciseName,
        int setOrderNumber,
        string text,
        FeedbackTone tone,
        bool isPublic,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? readAt,
        string? mediaReferenceUrl)
    {
        Id = id;
        CoachId = coachId;
        CoachName = coachName;
        StudentId = studentId;
        WorkoutSessionId = workoutSessionId;
        WorkoutExerciseId = workoutExerciseId;
        WorkoutSetId = workoutSetId;
        ExerciseName = exerciseName;
        SetOrderNumber = setOrderNumber;
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ReadAt = readAt;
        MediaReferenceUrl = mediaReferenceUrl;
    }

    public FeedbackLevel Level => FeedbackLevel.Set;
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string? CoachName { get; init; }
    public Guid StudentId { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public Guid WorkoutExerciseId { get; init; }
    public Guid WorkoutSetId { get; init; }
    public string? ExerciseName { get; init; }
    public int SetOrderNumber { get; init; }
    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; }
    public bool IsPublic { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public string? MediaReferenceUrl { get; init; }
}

public sealed record FeedbacksBySessionBundleResponse
{
    public FeedbacksBySessionBundleResponse() { }

    public FeedbacksBySessionBundleResponse(
        Guid workoutSessionId,
        List<WorkoutFeedbackResponse> sessionFeedbacks,
        List<ExerciseFeedbackResponse> exerciseFeedbacks,
        List<SetFeedbackResponse> setFeedbacks)
    {
        WorkoutSessionId = workoutSessionId;
        SessionFeedbacks = sessionFeedbacks;
        ExerciseFeedbacks = exerciseFeedbacks;
        SetFeedbacks = setFeedbacks;
    }

    public Guid WorkoutSessionId { get; init; }
    public List<WorkoutFeedbackResponse> SessionFeedbacks { get; init; } = [];
    public List<ExerciseFeedbackResponse> ExerciseFeedbacks { get; init; } = [];
    public List<SetFeedbackResponse> SetFeedbacks { get; init; } = [];
    public int TotalCount => SessionFeedbacks.Count + ExerciseFeedbacks.Count + SetFeedbacks.Count;
}

/// <summary>
/// Item genérico de feedback com os 3 níveis serializados.
/// Usado nas listas paginadas "meus feedbacks" / "feedbacks enviados".
/// </summary>
public sealed record UnifiedFeedbackItemResponse
{
    public UnifiedFeedbackItemResponse() { }

    public UnifiedFeedbackItemResponse(
        Guid id,
        FeedbackLevel level,
        Guid coachId,
        string? coachName,
        Guid studentId,
        string? studentName,
        Guid workoutSessionId,
        string? sessionName,
        Guid? workoutExerciseId,
        string? exerciseName,
        Guid? workoutSetId,
        int? setOrderNumber,
        string text,
        FeedbackTone tone,
        bool isPublic,
        bool isRead,
        DateTimeOffset createdAt,
        DateTimeOffset? readAt,
        string? mediaReferenceUrl,
        string? studentResponseText,
        DateTimeOffset? studentRespondedAt)
    {
        Id = id;
        Level = level;
        CoachId = coachId;
        CoachName = coachName;
        StudentId = studentId;
        StudentName = studentName;
        WorkoutSessionId = workoutSessionId;
        SessionName = sessionName;
        WorkoutExerciseId = workoutExerciseId;
        ExerciseName = exerciseName;
        WorkoutSetId = workoutSetId;
        SetOrderNumber = setOrderNumber;
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        IsRead = isRead;
        CreatedAt = createdAt;
        ReadAt = readAt;
        MediaReferenceUrl = mediaReferenceUrl;
        StudentResponseText = studentResponseText;
        StudentRespondedAt = studentRespondedAt;
    }

    public Guid Id { get; init; }
    public FeedbackLevel Level { get; init; }
    public Guid CoachId { get; init; }
    public string? CoachName { get; init; }
    public Guid StudentId { get; init; }
    public string? StudentName { get; init; }
    public Guid WorkoutSessionId { get; init; }
    public string? SessionName { get; init; }
    public Guid? WorkoutExerciseId { get; init; }
    public string? ExerciseName { get; init; }
    public Guid? WorkoutSetId { get; init; }
    public int? SetOrderNumber { get; init; }
    public string Text { get; init; } = string.Empty;
    public FeedbackTone Tone { get; init; }
    public bool IsPublic { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public string? MediaReferenceUrl { get; init; }
    public string? StudentResponseText { get; init; }
    public DateTimeOffset? StudentRespondedAt { get; init; }
}
