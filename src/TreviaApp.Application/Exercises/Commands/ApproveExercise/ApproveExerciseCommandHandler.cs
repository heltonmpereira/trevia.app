namespace TreviaApp.Application.Exercises.Commands.ApproveExercise;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;

public sealed class ApproveExerciseCommandHandler : ICommandHandler<ApproveExerciseCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ApproveExerciseCommandHandler> _logger;

    public ApproveExerciseCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<ApproveExerciseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task Handle(ApproveExerciseCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        if (!_currentUser.IsInRole(AppRoles.Administrator))
            throw new DomainException("Apenas administradores podem aprovar exercícios.", ErrorCodes.Forbidden);

        var exercise = await _db.Set<Exercise>()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            throw new DomainException("Exercício não encontrado.", ErrorCodes.ExerciseNotFound);

        exercise.Approve(adminUserId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("ApproveExerciseHandler: SaveChangesAsync explícito concluído ExerciseId={ExerciseId}", exercise.Id);

        _logger.LogInformation("ExerciseApproved ExerciseId={Id} AdminUserId={AdminId}", exercise.Id, adminUserId);
    }
}
