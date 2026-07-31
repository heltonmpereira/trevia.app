namespace TreviaApp.Application.Coaching.Queries.GetCoachRelationshipById;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Profiles;

public sealed class GetCoachRelationshipByIdQueryHandler : IQueryHandler<GetCoachRelationshipByIdQuery, CoachStudentLinkResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetCoachRelationshipByIdQueryHandler> _logger;

    public GetCoachRelationshipByIdQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetCoachRelationshipByIdQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<CoachStudentLinkResponse> Handle(GetCoachRelationshipByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var link = await _db.Set<CoachStudentLink>()
            .Include(l => l.Coach)
            .Include(l => l.Student)
            .FirstOrDefaultAsync(l => l.Id == request.LinkId, cancellationToken);

        if (link is null)
            throw new DomainException("Vínculo coach-aluno não encontrado.", ErrorCodes.CoachLinkNotFound);

        var isAdmin = _currentUser.IsInRole(AppRoles.Administrator) || _currentUser.IsInRole(AppRoles.GymManager);
        if (!isAdmin && link.CoachId != userId && link.StudentId != userId)
            throw new DomainException("Você não tem permissão para acessar este vínculo.", ErrorCodes.Forbidden);

        var profilePhotos = await _db.Set<UserProfile>()
            .Where(up => up.UserId == link.CoachId || up.UserId == link.StudentId)
            .Select(up => new { up.UserId, PhotoFileId = up.Photo != null ? up.Photo.FileId : null })
            .ToDictionaryAsync(k => k.UserId, v => v.PhotoFileId, cancellationToken);

        string? coachPhotoFileId = profilePhotos.TryGetValue(link.CoachId, out var cp) ? cp : null;
        string? studentPhotoFileId = profilePhotos.TryGetValue(link.StudentId, out var sp) ? sp : null;

        return new CoachStudentLinkResponse(
            link.Id,
            link.CoachId,
            $"{link.Coach.FirstName} {link.Coach.LastName}",
            coachPhotoFileId,
            link.StudentId,
            $"{link.Student.FirstName} {link.Student.LastName}",
            studentPhotoFileId,
            link.Permissions,
            link.IsActive,
            link.StartedAt,
            link.EndedAt,
            link.EndReason,
            link.EndReasonNotes);
    }
}
