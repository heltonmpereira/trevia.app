using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Coaching;

/// <summary>
/// Represents the CoachStudentLink domain entity.
/// </summary>
public class CoachStudentLink : AggregateRoot
{
    /// <summary>
    /// Gets Coach Id.
    /// </summary>
    public Guid CoachId { get; private set; }
    /// <summary>
    /// Gets Coach.
    /// </summary>
    public AppUser Coach { get; private set; } = null!;

    /// <summary>
    /// Gets Student Id.
    /// </summary>
    public Guid StudentId { get; private set; }
    /// <summary>
    /// Gets Student.
    /// </summary>
    public AppUser Student { get; private set; } = null!;

    /// <summary>
    /// Gets Permissions.
    /// </summary>
    public CoachPermissions Permissions { get; private set; }

    /// <summary>
    /// Gets Started At.
    /// </summary>
    public DateTimeOffset StartedAt { get; private set; }
    /// <summary>
    /// Gets Ended At.
    /// </summary>
    public DateTimeOffset? EndedAt { get; private set; }
    /// <summary>
    /// Gets End Reason.
    /// </summary>
    public CoachRelationshipEndReason? EndReason { get; private set; }
    /// <summary>
    /// Gets End Reason Notes.
    /// </summary>
    public string? EndReasonNotes { get; private set; }

    /// <summary>
    /// Gets Is Active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets Originating Coach Request Id.
    /// </summary>
    public Guid? OriginatingCoachRequestId { get; private set; }

    private CoachStudentLink() { }

    /// <summary>
    /// Initializes a new instance of the CoachStudentLink class.
    /// </summary>
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

    /// <summary>
    /// Executes Update Permissions.
    /// </summary>
    public void UpdatePermissions(CoachPermissions newPermissions, Guid updatedByCoachId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update permissions on inactive link.");
        if (updatedByCoachId != CoachId)
            throw new UnauthorizedAccessException("Only the coach of this relationship can update permissions.");

        Permissions = newPermissions;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Has Permission.
    /// </summary>
    public bool HasPermission(CoachPermissions permission)
    {
        return Permissions.HasFlag(permission);
    }

    /// <summary>
    /// Executes Grant Permission.
    /// </summary>
    public void GrantPermission(CoachPermissions permission, Guid updatedByCoachId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot grant permission on inactive link.");
        if (updatedByCoachId != CoachId)
            throw new UnauthorizedAccessException("Only the coach of this relationship can grant permissions.");

        Permissions |= permission;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Revoke Permission.
    /// </summary>
    public void RevokePermission(CoachPermissions permission, Guid updatedByCoachId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot revoke permission on inactive link.");
        if (updatedByCoachId != CoachId)
            throw new UnauthorizedAccessException("Only the coach of this relationship can revoke permissions.");

        Permissions &= ~permission;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes End Relationship.
    /// </summary>
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
