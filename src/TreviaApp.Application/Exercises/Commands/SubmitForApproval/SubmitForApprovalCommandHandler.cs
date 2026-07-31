namespace TreviaApp.Application.Exercises.Commands.SubmitForApproval;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class SubmitForApprovalCommandHandler : ICommandHandler<SubmitForApprovalCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SubmitForApprovalCommandHandler> _logger;

    public SubmitForApprovalCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<SubmitForApprovalCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(SubmitForApprovalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var exercise = await _db.Set<Exercise>()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        if (exercise.CreatedByUserId != userId)
            throw new DomainException("Apenas o proprietário pode submeter para aprovação.", ErrorCodes.ExerciseNotOwner);

        exercise.SubmitForApproval();

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("SubmitForApprovalHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation("ExerciseSubmittedForApproval ExerciseId={Id} UserId={UserId}", exercise.Id, userId);
    }
}
