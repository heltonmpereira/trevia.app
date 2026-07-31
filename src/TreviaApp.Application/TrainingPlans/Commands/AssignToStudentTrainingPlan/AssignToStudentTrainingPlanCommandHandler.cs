namespace TreviaApp.Application.TrainingPlans.Commands.AssignToStudentTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class AssignToStudentTrainingPlanCommandHandler : ICommandHandler<AssignToStudentTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AssignToStudentTrainingPlanCommandHandler> _logger;

    public AssignToStudentTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<AssignToStudentTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(AssignToStudentTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var tp = await _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (tp is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdminOrGymManager(tp, userId))
            throw new DomainException("Você não tem permissão para atribuir este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        if (tp.AssignedToStudentId.HasValue)
            throw new DomainException("Este plano de treino já foi atribuído a um aluno e não pode ser editado/reatribuído.", ErrorCodes.TrainingPlanNotEditable);

        tp.AssignToStudent(request.StudentId);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("AssignToStudentTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId} StudentId={StudentId}", tp.Id, request.StudentId);

        _logger.LogInformation(
            "TrainingPlanAssigned TrainingPlanId={TrainingPlanId} UserId={UserId} StudentId={StudentId}",
            tp.Id,
            userId,
            request.StudentId);

        return TrainingPlanMappings.MapToDetail(tp, null, null, false);
    }

    private bool IsOwnerOrAdminOrGymManager(TrainingPlan tp, Guid? userId)
    {
        return (userId.HasValue && userId == tp.CreatedByUserId)
               || _currentUser.IsInRole(AppRoles.Administrator)
               || _currentUser.IsInRole(AppRoles.GymManager);
    }
}
