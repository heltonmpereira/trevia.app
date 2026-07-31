namespace TreviaApp.Application.Consents.Commands.RevokeConsent;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class RevokeConsentCommandHandler : ICommandHandler<RevokeConsentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RevokeConsentCommandHandler> _logger;

    public RevokeConsentCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<RevokeConsentCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(RevokeConsentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            throw new DomainException("Usuário não autenticado.", ErrorCodes.Unauthorized);

        var activeConsents = await _db.Set<UserConsent>()
            .Where(c => c.UserId == userId.Value &&
                        c.ConsentType == request.ConsentType &&
                        !c.RevokedAt.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var c in activeConsents)
        {
            c.Revoke(request.Reason);
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("RevokeConsentHandler: SaveChangesAsync explícito concluído para UserId={UserId} Type={ConsentType}", userId.Value, request.ConsentType);

        _logger.LogInformation("UserRevokedConsent UserId={UserId} Type={ConsentType}", userId.Value, request.ConsentType);
    }
}
