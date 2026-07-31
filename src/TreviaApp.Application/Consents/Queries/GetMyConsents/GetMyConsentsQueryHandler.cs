namespace TreviaApp.Application.Consents.Queries.GetMyConsents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Consents.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class GetMyConsentsQueryHandler : IQueryHandler<GetMyConsentsQuery, List<ConsentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyConsentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<ConsentResponse>> Handle(GetMyConsentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            throw new DomainException("Usuário não autenticado.", ErrorCodes.Unauthorized);

        var query = _db.Set<UserConsent>()
            .Where(c => c.UserId == userId.Value)
            .OrderByDescending(c => c.AcceptedAt)
            .AsQueryable();

        if (request.IncludeRevoked == false)
        {
            query = query.Where(c => !c.IsRevoked);
        }

        var consents = await query.ToListAsync(cancellationToken);

        return consents.Select(c => new ConsentResponse(
            c.Id,
            c.ConsentType,
            c.ConsentVersion,
            c.AcceptedAt,
            c.IsRevoked,
            c.RevokedAt,
            c.RevocationReason)).ToList();
    }
}
