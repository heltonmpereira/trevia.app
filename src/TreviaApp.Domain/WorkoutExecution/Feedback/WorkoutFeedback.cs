using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.WorkoutExecution.Feedback;

/// <summary>
/// Feedback de nível de sessão de treino completo.
/// US-1001: Professor com CanSendFeedback envia comentário global sobre a sessão do aluno.
/// </summary>
public class WorkoutFeedback : AggregateRoot
{
    public Guid CoachId { get; private set; }
    public AppUser Coach { get; private set; } = null!;

    public Guid StudentId { get; private set; }
    public AppUser Student { get; private set; } = null!;

    public Guid WorkoutSessionId { get; private set; }
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    public string Text { get; private set; } = string.Empty;
    public FeedbackTone Tone { get; private set; }
    public bool IsPublic { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    private WorkoutFeedback() { }

    public WorkoutFeedback(
        Guid coachId,
        Guid studentId,
        Guid workoutSessionId,
        string text,
        FeedbackTone tone,
        bool isPublic = true)
    {
        if (coachId == Guid.Empty) throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        if (workoutSessionId == Guid.Empty) throw new ArgumentException("WorkoutSessionId cannot be empty.", nameof(workoutSessionId));
        if (coachId == studentId) throw new InvalidOperationException("Coach and student cannot be the same user.");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Feedback text cannot be empty.", nameof(text));
        if (text.Length > 4000) throw new ArgumentException("Feedback text cannot exceed 4000 characters.", nameof(text));

        CoachId = coachId;
        StudentId = studentId;
        WorkoutSessionId = workoutSessionId;
        Text = text;
        Tone = tone;
        IsPublic = isPublic;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContent(string text, FeedbackTone tone, Guid updatedByCoachId, bool? isPublic = null)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot edit deleted feedback.");
        if (updatedByCoachId != CoachId) throw new UnauthorizedAccessException("Only the original coach can edit this feedback.");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Feedback text cannot be empty.", nameof(text));
        if (text.Length > 4000) throw new ArgumentException("Feedback text cannot exceed 4000 characters.", nameof(text));

        Text = text;
        Tone = tone;
        if (isPublic.HasValue) IsPublic = isPublic.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRead(Guid studentReaderId)
    {
        if (ReadAt.HasValue) return;
        if (studentReaderId != StudentId) throw new UnauthorizedAccessException("Only the target student can mark this feedback as read.");
        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public FeedbackLevel Level => FeedbackLevel.Session;
}
