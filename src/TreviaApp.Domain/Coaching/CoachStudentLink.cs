using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Coaching;

public class CoachStudentLink : AggregateRoot
{
    public Guid CoachId { get; private set; }
    public AppUser Coach { get; private set; } = null!;

    public Guid StudentId { get; private set; }
    public AppUser Student { get; private set; } = null!;

    public CoachPermissions Permissions { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public CoachRelationshipEndReason? EndReason { get; private set; }
    public string? EndReasonNotes { get; private set; }

    public bool IsActive { get; private set; }

    public Guid? OriginatingCoachRequestId { get; private set; }

    private CoachStudentLink() { }

    public CoachStudentLink(
        Guid coachId,
        Guid studentId,
        CoachPermissions? initialPermissions = null,
        Guid? originatingCoachRequestId = null)
    {
        if (coachId == Guid.Empty) throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        if (coachId == studentId) throw new InvalidOperationException("Coach and student cannot be the same user.");

        CoachId = coachId;
        StudentId = studentId;
        Permissions = initialPermissions ??
            CoachPermissions.CanViewWeightHistory |
            CoachPermissions.CanViewBodyMeasurements |
            CoachPermissions.CanViewProfilePhotos |
            CoachPermissions.CanAssignTrainingPlans |
            CoachPermissions.CanViewWorkoutHistory;
        StartedAt = DateTimeOffset.UtcNow;
        IsActive = true;
        OriginatingCoachRequestId = originatingCoachRequestId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePermissions(CoachPermissions newPermissions, Guid updatedByCoachId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update permissions on inactive link.");
        if (updatedByCoachId != CoachId)
            throw new UnauthorizedAccessException("Only the coach of this relationship can update permissions.");

        Permissions = newPermissions;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool HasPermission(CoachPermissions permission)
    {
        return Permissions.HasFlag(permission);
    }

    public void GrantPermission(CoachPermissions permission, Guid updatedByCoachId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot grant permission on inactive link.");
        if (updatedByCoachId != CoachId)
            throw new UnauthorizedAccessException("Only the coach of this relationship can grant permissions.");

        Permissions |= permission;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RevokePermission(CoachPermissions permission, Guid updatedByCoachId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot revoke permission on inactive link.");
        if (updatedByCoachId != CoachId)
            throw new UnauthorizedAccessException("Only the coach of this relationship can revoke permissions.");

        Permissions &= ~permission;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void EndRelationship(Guid endedByUserId, CoachRelationshipEndReason reason, string? notes = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("This link is already inactive.");
        if (endedByUserId == Guid.Empty)
            throw new ArgumentException("EndedByUserId cannot be empty.", nameof(endedByUserId));

        if (endedByUserId != CoachId && endedByUserId != StudentId)
            throw new UnauthorizedAccessException("Only the coach, student, or admin can end this relationship.");

        if (notes != null && notes.Length > 1000)
            throw new ArgumentException("EndReasonNotes too long (> 1000).", nameof(notes));

        IsActive = false;
        EndedAt = DateTimeOffset.UtcNow;
        EndReason = reason;
        EndReasonNotes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
