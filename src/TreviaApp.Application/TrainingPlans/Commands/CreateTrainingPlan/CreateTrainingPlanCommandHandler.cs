namespace TreviaApp.Application.TrainingPlans.Commands.CreateTrainingPlan;

using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Application.TrainingPlans.Mappings;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.TrainingPlans;
using TreviaApp.Shared.Constants;

public sealed class CreateTrainingPlanCommandHandler : ICommandHandler<CreateTrainingPlanCommand, TrainingPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateTrainingPlanCommandHandler> _logger;

    public CreateTrainingPlanCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CreateTrainingPlanCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TrainingPlanDetailResponse> Handle(CreateTrainingPlanCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            ErrorCodes.Unauthorized);

        var tp = new TrainingPlan(
            userId,
            request.Name,
            request.SplitType,
            TreviaApp.Shared.Enums.TrainingPlanStatus.Draft,
            request.Visibility);

        tp.UpdateBasicInfo(
            request.Name,
            request.Description,
            request.InstructionsIntro,
            request.NotesForStudent,
            request.Tags,
            request.SplitType,
            request.Visibility,
            request.TotalWeeks,
            request.SessionsPerWeek);

        _db.Set<TrainingPlan>().Add(tp);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("CreateTrainingPlanHandler: SaveChangesAsync explícito concluído TrainingPlanId={TrainingPlanId}", tp.Id);

        _logger.LogInformation(
            "TrainingPlanCreated TrainingPlanId={TrainingPlanId} UserId={UserId} Name={Name}",
            tp.Id,
            userId,
            tp.Name);

        return TrainingPlanMappings.MapToDetail(tp, null, null, false);
    }
}
