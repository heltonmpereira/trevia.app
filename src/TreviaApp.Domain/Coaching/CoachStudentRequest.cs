using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Coaching;

public class CoachStudentRequest : AggregateRoot
{
    public Guid CoachId { get; private set; }
    public AppUser Coach { get; private set; } = null!;

    public Guid StudentId { get; private set; }
    public AppUser Student { get; private set; } = null!;

    public CoachInviteDirection Direction { get; private set; }
    public CoachRequestStatus Status { get; private set; } = CoachRequestStatus.Pending;

    public string? Message { get; private set; }
    public string? CoachNotesInternal { get; private set; }
    public string? RejectionReason { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }
    public Guid? RespondedByUserId { get; private set; }

    public CoachPermissions GrantedPermissionsOnAccept { get; private set; } =
        CoachPermissions.CanViewWeightHistory |
        CoachPermissions.CanViewBodyMeasurements |
        CoachPermissions.CanViewProfilePhotos |
        CoachPermissions.CanAssignTrainingPlans |
        CoachPermissions.CanViewWorkoutHistory;

    private CoachStudentRequest() { }

    public CoachStudentRequest(
        Guid coachId,
        Guid studentId,
        CoachInviteDirection direction,
        string? message = null,
        int expiresInDays = 30,
        CoachPermissions? grantedPermissionsOnAccept = null)
    {
        if (coachId == Guid.Empty) throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (studentId == Guid.Empty) throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        if (coachId == studentId) throw new InvalidOperationException("Coach and student cannot be the same user.");
        if (expiresInDays <= 0) throw new ArgumentOutOfRangeException(nameof(expiresInDays), "ExpiresInDays must be positive.");
        if (message != null && message.Length > 500) throw new ArgumentException("Message too long (> 500).", nameof(message));

        CoachId = coachId;
        StudentId = studentId;
        Direction = direction;
        Message = message;
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(expiresInDays);

        if (grantedPermissionsOnAccept.HasValue)
            GrantedPermissionsOnAccept = grantedPermissionsOnAccept.Value;

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt && Status == CoachRequestStatus.Pending;

    public void Accept(Guid acceptedByUserId)
    {
        if (Status != CoachRequestStatus.Pending)
            throw new InvalidOperationException($"Cannot accept request in status {Status}.");
        if (IsExpired)
            throw new InvalidOperationException("Cannot accept expired request.");
        if (acceptedByUserId == Guid.Empty)
            throw new ArgumentException("AcceptedByUserId cannot be empty.", nameof(acceptedByUserId));

        Status = CoachRequestStatus.Accepted;
        RespondedAt = DateTimeOffset.UtcNow;
        RespondedByUserId = acceptedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid rejectedByUserId, string? reason = null)
    {
        if (Status != CoachRequestStatus.Pending)
            throw new InvalidOperationException($"Cannot reject request in status {Status}.");
        if (rejectedByUserId == Guid.Empty)
            throw new ArgumentException("RejectedByUserId cannot be empty.", nameof(rejectedByUserId));
        if (reason != null && reason.Length > 500)
            throw new ArgumentException("Rejection reason too long (> 500).", nameof(reason));

        Status = CoachRequestStatus.Rejected;
        RejectionReason = reason;
        RespondedAt = DateTimeOffset.UtcNow;
        RespondedByUserId = rejectedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(Guid cancelledByUserId)
    {
        if (Status != CoachRequestStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel request in status {Status}.");
        if (cancelledByUserId == Guid.Empty)
            throw new ArgumentException("CancelledByUserId cannot be empty.", nameof(cancelledByUserId));

        Status = CoachRequestStatus.Cancelled;
        RespondedAt = DateTimeOffset.UtcNow;
        RespondedByUserId = cancelledByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCoachNotesInternal(string? notes)
    {
        if (notes != null && notes.Length > 1000)
            throw new ArgumentException("CoachNotesInternal too long (> 1000).", nameof(notes));

        CoachNotesInternal = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
