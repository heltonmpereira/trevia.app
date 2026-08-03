using TreviaApp.Contracts.Coaching.Requests;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services.Coaching;

public interface ICoachingService
{
    Task<CoachInviteResponse> SendCoachInvite(SendCoachInviteRequest request, CancellationToken ct = default);
    Task<CoachInviteResponse> SendStudentRequest(SendStudentRequestRequest request, CancellationToken ct = default);
    Task<CoachStudentLinkResponse> AcceptInvite(Guid inviteId, AcceptCoachInviteRequest? request = null, CancellationToken ct = default);
    Task<CoachInviteResponse> RejectInvite(Guid inviteId, RejectCoachInviteRequest? request = null, CancellationToken ct = default);
    Task<CoachInviteResponse> CancelInvite(Guid inviteId, CancellationToken ct = default);
    Task<CoachStudentLinkResponse> EndRelationship(Guid linkId, CoachRelationshipEndReason reason, string? notes = null, CancellationToken ct = default);
    Task<CoachStudentLinkResponse> UpdatePermissions(Guid linkId, UpdateCoachPermissionsRequest request, CancellationToken ct = default);

    Task<CoachingInvitesPagedResponse> GetIncomingInvites(int page = 1, int pageSize = 10, CoachRequestStatus? statusFilter = null, string? sortBy = "createdAtDesc", CancellationToken ct = default);
    Task<CoachingInvitesPagedResponse> GetOutgoingInvites(int page = 1, int pageSize = 10, CoachRequestStatus? statusFilter = null, string? sortBy = "createdAtDesc", CancellationToken ct = default);
    Task<int> GetPendingInvitesCount(CancellationToken ct = default);

    Task<CoachStudentsPagedResponse> GetMyStudents(int page = 1, int pageSize = 10, string? searchName = null, bool? onlyActive = true, string? sortBy = "linkedSinceDesc", CancellationToken ct = default);
    Task<CoachStudentsPagedResponse> GetMyCoaches(int page = 1, int pageSize = 10, string? searchName = null, bool? onlyActive = true, string? sortBy = "linkedSinceDesc", CancellationToken ct = default);

    Task<CoachStudentLinkResponse> GetRelationshipById(Guid linkId, CancellationToken ct = default);
    Task<CoachLinkStatusResponse> CheckLinkStatus(Guid otherUserId, CancellationToken ct = default);

    Task<CoachStudentsPagedResponse> GetCoachStudentsAsAdmin(Guid coachId, int page = 1, int pageSize = 10, string? searchName = null, bool onlyActive = true, CancellationToken ct = default);

    Task<CoachStudentsPagedResponse> SearchStudentsNotLinked(string? searchName = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<CoachStudentsPagedResponse> SearchCoachesNotLinked(string? searchName = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
