using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.WorkoutExecution.Feedback;

/// <summary>
/// Feedback de nível de série individual.
/// US-1003: Comentário sobre uma série específica (técnica, carga, execução).
/// Suporta referência futura a mídia (vídeo de análise de movimento).
/// </summary>
public class SetFeedback : AggregateRoot
{
    public Guid CoachId { get; private set; }
    public AppUser Coach { get; private set; } = null!;

    public Guid StudentId { get; private set; }
    public AppUser Student { get; private set; } = null!;

    public Guid WorkoutSessionId { get; private set; }
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    public Guid WorkoutExerciseId { get; private set; }
    public WorkoutExercise WorkoutExercise { get; private set; } = null!;

    public Guid WorkoutSetId { get; private set; }
    public WorkoutSet WorkoutSet { get; private set; } = null!;

    public string Text { get; private set; } = string.Empty;
    public FeedbackTone Tone { get; private set; }
    public bool IsPublic { get; private set; }

    /// <summary>
    /// URL de referência para mídia (vídeo/imagem). Preparatório (MVP não implementa upload).
    /// </summary>
    public string? MediaReferenceUrl { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    private SetFeedback() { }

    public SetFeedback(
        Guid coachId,
        Guid studentId,
        Guid workoutSessionId,
        Guid workoutExerciseId,
        Guid workoutSetId,
        string text,
        FeedbackTone tone,
        bool isPublic = true,
        string? mediaReferenceUrl = null)
    {
        if (coachId == Guid.Empty) throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("WorkoutSessionId cannot be empty.", nameof(workoutSessionId));
        if (workoutExerciseId == Guid.Empty) throw new ArgumentException("WorkoutExerciseId cannot be empty.", nameof(workoutExerciseId));
        if (workoutSetId == Guid.Empty) throw new ArgumentException("WorkoutSetId cannot be empty.", nameof(workoutSetId));
        if (coachId == studentId) throw new InvalidOperationException("Coach and student cannot be the same user.");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Feedback text cannot be empty.", nameof(text));
        if (text.Length > 4000) throw new ArgumentException("Feedback text cannot exceed 4000 characters.", nameof(text));
        if (!string.IsNullOrEmpty(mediaReferenceUrl) && mediaReferenceUrl.Length > 2048)
            throw new ArgumentException("MediaReferenceUrl cannot exceed 2048 characters.", nameof(mediaReferenceUrl));

        CoachId = coachId;
        StudentId = studentId;
        WorkoutSessionId = workoutSessionId;
        WorkoutExerciseId = workoutExerciseId;
        WorkoutSetId = workoutSetId;
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        MediaReferenceUrl = mediaReferenceUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContent(
        string text,
        FeedbackTone tone,
        Guid updatedByCoachId,
        bool? isPublic = null,
        string? mediaReferenceUrl = null)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot edit deleted feedback.");
        if (updatedByCoachId != CoachId) throw new UnauthorizedAccessException("Only the original coach can edit this feedback.");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Feedback text cannot be empty.", nameof(text));
        if (text.Length > 4000) throw new ArgumentException("Feedback text cannot exceed 4000 characters.", nameof(text));
        if (!string.IsNullOrEmpty(mediaReferenceUrl) && mediaReferenceUrl.Length > 2048)
            throw new ArgumentException("MediaReferenceUrl cannot exceed 2048 characters.", nameof(mediaReferenceUrl));

        Text = text;
        Tone = tone;
        if (isPublic.HasValue) IsPublic = isPublic.Value;
        if (mediaReferenceUrl != null) MediaReferenceUrl = mediaReferenceUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRead(Guid studentReaderId)
    {
        if (ReadAt.HasValue) return;
        if (studentReaderId != StudentId) throw new UnauthorizedAccessException("Only the target student can mark this feedback as read.");
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public FeedbackLevel Level => FeedbackLevel.Set;
}
