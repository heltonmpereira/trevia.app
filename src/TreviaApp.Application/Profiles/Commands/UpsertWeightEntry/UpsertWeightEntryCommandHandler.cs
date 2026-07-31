namespace TreviaApp.Application.Profiles.Commands.UpsertWeightEntry;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class UpsertWeightEntryCommandHandler : ICommandHandler<UpsertWeightEntryCommand, WeightEntryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpsertWeightEntryCommandHandler> _logger;

    public UpsertWeightEntryCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<UpsertWeightEntryCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<WeightEntryResponse> Handle(UpsertWeightEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var profile = await _db.Set<UserProfile>()
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            throw new DomainException(
                "Perfil não encontrado.",
                "ProfileNotFound");

        profile.AddWeightEntry(request.WeightKg, request.MeasuredAt, request.Note);

        var latestEntry = profile.WeightEntries
            .OrderByDescending(w => w.MeasuredAt)
            .FirstOrDefault(w => w.MeasuredAt == request.MeasuredAt && w.WeightKg == request.WeightKg);

        if (latestEntry is null)
        {
            latestEntry = await _db.Set<WeightEntry>()
                .OrderByDescending(w => w.CreatedAt)
                .FirstOrDefaultAsync(w => w.ProfileId == profile.Id, cancellationToken);
        }

        if (latestEntry is null)
            throw new InvalidOperationException("Falha ao adicionar entrada de peso.");

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("UpsertWeightEntryHandler: SaveChangesAsync explícito concluído para ProfileId={ProfileId} EntryId={EntryId}", profile.Id, latestEntry.Id);

        _logger.LogInformation(
            "WeightEntryAdded ProfileId={ProfileId} EntryId={EntryId} WeightKg={WeightKg}",
            profile.Id,
            latestEntry.Id,
            request.WeightKg);

        return ProfileMappings.MapToWeightEntryResponse(latestEntry);
    }
}
