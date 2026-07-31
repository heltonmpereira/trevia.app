namespace TreviaApp.Application.TrainingPlans.Commands.DuplicateTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class DuplicateTrainingPlanCommandHandler : ICommandHandler<DuplicateTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DuplicateTrainingPlanCommandHandler> _logger;

    public DuplicateTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<DuplicateTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(DuplicateTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var original = await _db.Set<TrainingPlan>()
            .Include(tp => tp.Sessions)
            .ThenInclude(s => s.Exercises)
            .ThenInclude(e => e.Prescriptions)
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId, cancellationToken);

        if (original is null)
            throw new DomainException("Plano de treino não encontrado.", ErrorCodes.TrainingPlanNotFound);

        if (!IsOwnerOrAdmin(original, userId))
            throw new DomainException("Você não tem permissão para duplicar este plano de treino.", ErrorCodes.TrainingPlanNotOwner);

        Guid newOwnerId = request.AssignToMe ? userId : original.CreatedByUserId;

        var copy = original.Duplicate(newOwnerId, keepStatusDraft: true);

        if (!string.IsNullOrWhiteSpace(request.NewName))
        {
            copy.UpdateBasicInfo(
                request.NewName,
                copy.Description,
                copy.InstructionsIntro,
                copy.NotesForStudent,
                copy.Tags,
                copy.SplitType,
                copy.Visibility,
                copy.TotalWeeks,
                copy.SessionsPerWeek);
        }

        if (request.AssignToMe && _currentUser.IsInRole(AppRoles.Student))
        {
            copy.AssignToStudent(copy.CreatedByUserId);
        }

        _db.Set<TrainingPlan>().Add(copy);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("DuplicateTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId} CopyId={CopyId}", original.Id, copy.Id);

        _logger.LogInformation(
            "TrainingPlanDuplicated OriginalId={OriginalId} CopyId={CopyId} UserId={UserId}",
            original.Id,
            copy.Id,
            userId);

        return TrainingPlanMappings.MapToDetail(copy, null, null, false);
    }

    private bool IsOwnerOrAdmin(TrainingPlan tp, Guid userId)
    {
        return userId == tp.CreatedByUserId
               || _currentUser.IsInRole(AppRoles.Administrator);
    }
}
