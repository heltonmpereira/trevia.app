namespace TreviaApp.Application.Profiles.Commands.DeleteWeightEntry;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class DeleteWeightEntryCommandHandler : ICommandHandler<DeleteWeightEntryCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteWeightEntryCommandHandler> _logger;

    public DeleteWeightEntryCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<DeleteWeightEntryCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(DeleteWeightEntryCommand request, CancellationToken cancellationToken)
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

        profile.RemoveWeightEntry(request.WeightEntryId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("DeleteWeightEntryHandler: SaveChangesAsync explícito concluído para ProfileId={ProfileId} EntryId={EntryId}", profile.Id, request.WeightEntryId);

        _logger.LogInformation(
            "WeightEntryRemoved ProfileId={ProfileId} EntryId={EntryId}",
            profile.Id,
            request.WeightEntryId);
    }
}
