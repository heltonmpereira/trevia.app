using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Coaching;

/// <summary>
/// Represents the CoachStudentRequest domain entity.
/// </summary>
public class CoachStudentRequest : AggregateRoot
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
    /// Gets Direction.
    /// </summary>
    public CoachInviteDirection Direction { get; private set; }
    /// <summary>
    /// Gets Status.
    /// </summary>
    public CoachRequestStatus Status { get; private set; } = CoachRequestStatus.Pending;

    /// <summary>
    /// Gets Message.
    /// </summary>
    public string? Message { get; private set; }
    /// <summary>
    /// Gets Coach Notes Internal.
    /// </summary>
    public string? CoachNotesInternal { get; private set; }
    /// <summary>
    /// Gets Rejection Reason.
    /// </summary>
    public string? RejectionReason { get; private set; }

    /// <summary>
    /// Gets Expires At.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }
    /// <summary>
    /// Gets Responded At.
    /// </summary>
    public DateTimeOffset? RespondedAt { get; private set; }
    /// <summary>
    /// Gets Responded By User Id.
    /// </summary>
    public Guid? RespondedByUserId { get; private set; }

    /// <summary>
    /// Gets Granted Permissions On Accept.
    /// </summary>
    public CoachPermissions GrantedPermissionsOnAccept { get; private set; } =
        CoachPermissions.CanViewWeightHistory |
        CoachPermissions.CanViewBodyMeasurements |
        CoachPermissions.CanViewProfilePhotos |
        CoachPermissions.CanAssignTrainingPlans |
        CoachPermissions.CanViewWorkoutHistory;

    private CoachStudentRequest() { }

    /// <summary>
    /// Initializes a new instance of the CoachStudentRequest class.
    /// </summary>
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

    /// <summary>
    /// Gets Is Expired.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt && Status == CoachRequestStatus.Pending;

    /// <summary>
    /// Executes Accept.
    /// </summary>
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

    /// <summary>
    /// Executes Reject.
    /// </summary>
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

    /// <summary>
    /// Executes Cancel.
    /// </summary>
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

    /// <summary>
    /// Executes Set Coach Notes Internal.
    /// </summary>
    public void SetCoachNotesInternal(string? notes)
    {
        if (notes != null && notes.Length > 1000)
            throw new ArgumentException("CoachNotesInternal too long (> 1000).", nameof(notes));

        CoachNotesInternal = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
