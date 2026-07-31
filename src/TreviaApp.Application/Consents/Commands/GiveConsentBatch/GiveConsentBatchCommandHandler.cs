namespace TreviaApp.Application.Consents.Commands.GiveConsentBatch;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Contracts.Consents.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class GiveConsentBatchCommandHandler : ICommandHandler<GiveConsentBatchCommand, List<ConsentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<GiveConsentBatchCommandHandler> _logger;

    public GiveConsentBatchCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IHttpContextAccessor accessor,
        ILogger<GiveConsentBatchCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _accessor = accessor;
        _logger = logger;
    }

    public async Task<List<ConsentResponse>> Handle(GiveConsentBatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            throw new DomainException("Usuário não autenticado.", ErrorCodes.Unauthorized);

        var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? null;
        var ua = _accessor.HttpContext?.Request?.Headers["User-Agent"].FirstOrDefault();

        var distinctConsents = request.Consents
            .GroupBy(c => new { c.ConsentType, c.ConsentVersion })
            .Select(g => g.First())
            .ToList();

        var existingConsents = await _db.Set<UserConsent>()
            .Where(c => c.UserId == userId.Value)
            .ToListAsync(cancellationToken);

        var typesToCreate = new List<(TreviaApp.Shared.Enums.ConsentType Type, string Version)>();

        foreach (var consent in distinctConsents)
        {
            var existing = existingConsents.FirstOrDefault(c =>
                c.ConsentType == consent.ConsentType &&
                c.ConsentVersion == consent.ConsentVersion);

            if (existing is null || existing.IsRevoked)
            {
                var newConsent = new UserConsent(userId.Value, consent.ConsentType, consent.ConsentVersion, ip, ua);
                _db.Set<UserConsent>().Add(newConsent);
                typesToCreate.Add((consent.ConsentType, consent.ConsentVersion));
            }
        }

        var types = string.Join(",", distinctConsents.Select(c => c.ConsentType.ToString()));
        _logger.LogInformation("UserGaveConsents UserId={UserId} Types={types}", userId.Value, types);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("GiveConsentBatchHandler: SaveChangesAsync explícito concluído para UserId={UserId}", userId.Value);

        var resultTuples = typesToCreate.Any()
            ? typesToCreate
            : distinctConsents.Select(c => (c.ConsentType, c.ConsentVersion)).ToList();

        var createdConsents = await _db.Set<UserConsent>()
            .Where(c => c.UserId == userId.Value)
            .ToListAsync(cancellationToken);

        var responses = new List<ConsentResponse>();
        foreach (var (type, version) in resultTuples)
        {
            var match = createdConsents
                .OrderByDescending(c => c.AcceptedAt)
                .FirstOrDefault(c => c.ConsentType == type && c.ConsentVersion == version);

            if (match is not null)
            {
                responses.Add(new ConsentResponse(
                    match.Id,
                    match.ConsentType,
                    match.ConsentVersion,
                    match.AcceptedAt,
                    match.IsRevoked,
                    match.RevokedAt,
                    match.RevocationReason));
            }
        }

        return responses;
    }
}
