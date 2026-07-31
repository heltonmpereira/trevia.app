namespace TreviaApp.Application.Exercises.Commands.RejectExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class RejectExerciseCommandHandler : ICommandHandler<RejectExerciseCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RejectExerciseCommandHandler> _logger;

    public RejectExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<RejectExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(RejectExerciseCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        if (!_currentUser.IsInRole(AppRoles.Administrator))
            throw new DomainException("Apenas administradores podem rejeitar exercícios.", ErrorCodes.Forbidden);

        var exercise = await _db.Set<Exercise>()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        exercise.Reject(adminUserId, request.Reason);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("RejectExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation(
            "ExerciseRejected ExerciseId={Id} AdminUserId={AdminId} ReasonLength={ReasonLen}",
            exercise.Id,
            adminUserId,
            request.Reason?.Length ?? 0);
    }
}
