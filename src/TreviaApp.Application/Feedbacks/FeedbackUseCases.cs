using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Feedbacks.Responses;
using TreviaApp.Contracts.Notifications.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.Notifications;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Domain.WorkoutExecution.Feedback;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Application.Feedbacks;

#region ====================  COMMANDS (Create / Update / Delete / MarkRead / Respond)  ====================

public sealed record CreateWorkoutFeedbackCommand(
    Guid CurrentCoachId,
    bool ViewerIsAdminOrGymManager,
    Guid WorkoutSessionId,
    string Text,
    FeedbackTone Tone,
    bool IsPublic = true)
    : ICommand<WorkoutFeedbackResponse>;

public sealed record CreateExerciseFeedbackCommand(
    Guid CurrentCoachId,
    bool ViewerIsAdminOrGymManager,
    Guid WorkoutExerciseId,
    string Text,
    FeedbackTone Tone,
    bool IsPublic = true)
    : ICommand<ExerciseFeedbackResponse>;

public sealed record CreateSetFeedbackCommand(
    Guid CurrentCoachId,
    bool ViewerIsAdminOrGymManager,
    Guid WorkoutSetId,
    string Text,
    FeedbackTone Tone,
    bool IsPublic = true,
    string? MediaReferenceUrl = null)
    : ICommand<SetFeedbackResponse>;

public sealed record UpdateFeedbackCommand(
    Guid CurrentUserId,
    bool ViewerIsAdminOrGymManager,
    FeedbackLevel Level,
    Guid FeedbackId,
    string Text,
    FeedbackTone Tone,
    bool? IsPublic = null,
    string? MediaReferenceUrl = null)
    : ICommand<UnifiedFeedbackItemResponse>;

public sealed record DeleteFeedbackCommand(
    Guid CurrentUserId,
    bool ViewerIsAdminOrGymManager,
    FeedbackLevel Level,
    Guid FeedbackId)
    : ICommand<bool>;

public sealed record MarkFeedbackReadCommand(
    Guid CurrentUserId,
    FeedbackLevel Level,
    Guid FeedbackId)
    : ICommand<bool>;

public sealed record RespondToExerciseFeedbackCommand(
    Guid CurrentStudentUserId,
    Guid ExerciseFeedbackId,
    string ResponseText)
    : ICommand<ExerciseFeedbackResponse>;

#endregion

#region ====================  VALIDATORS  ====================

public sealed class CreateWorkoutFeedbackCommandValidator : AbstractValidator<CreateWorkoutFeedbackCommand>
{
    public CreateWorkoutFeedbackCommandValidator()
    {
        RuleFor(c => c.WorkoutSessionId).NotEmpty();
        RuleFor(c => c.Text)
            .NotEmpty().WithMessage("Feedback não pode ser vazio.")
            .WithErrorCode(ErrorCodes.FeedbackEmpty)
            .MaximumLength(4000).WithMessage("Feedback excede 4000 caracteres.")
            .WithErrorCode(ErrorCodes.FeedbackTextTooLong);
        RuleFor(c => c.Tone).IsInEnum();
    }
}

public sealed class CreateExerciseFeedbackCommandValidator : AbstractValidator<CreateExerciseFeedbackCommand>
{
    public CreateExerciseFeedbackCommandValidator()
    {
        RuleFor(c => c.WorkoutExerciseId).NotEmpty();
        RuleFor(c => c.Text)
            .NotEmpty().WithErrorCode(ErrorCodes.FeedbackEmpty)
            .MaximumLength(4000).WithErrorCode(ErrorCodes.FeedbackTextTooLong);
        RuleFor(c => c.Tone).IsInEnum();
    }
}

public sealed class CreateSetFeedbackCommandValidator : AbstractValidator<CreateSetFeedbackCommand>
{
    public CreateSetFeedbackCommandValidator()
    {
        RuleFor(c => c.WorkoutSetId).NotEmpty();
        RuleFor(c => c.Text)
            .NotEmpty().WithErrorCode(ErrorCodes.FeedbackEmpty)
            .MaximumLength(4000).WithErrorCode(ErrorCodes.FeedbackTextTooLong);
        RuleFor(c => c.MediaReferenceUrl).MaximumLength(2048);
        RuleFor(c => c.Tone).IsInEnum();
    }
}

public sealed class UpdateFeedbackCommandValidator : AbstractValidator<UpdateFeedbackCommand>
{
    public UpdateFeedbackCommandValidator()
    {
        RuleFor(c => c.FeedbackId).NotEmpty();
        RuleFor(c => c.Level).IsInEnum();
        RuleFor(c => c.Text)
            .NotEmpty().WithErrorCode(ErrorCodes.FeedbackEmpty)
            .MaximumLength(4000).WithErrorCode(ErrorCodes.FeedbackTextTooLong);
        RuleFor(c => c.MediaReferenceUrl).MaximumLength(2048);
    }
}

public sealed class RespondToExerciseFeedbackCommandValidator : AbstractValidator<RespondToExerciseFeedbackCommand>
{
    public RespondToExerciseFeedbackCommandValidator()
    {
        RuleFor(c => c.ExerciseFeedbackId).NotEmpty();
        RuleFor(c => c.ResponseText)
            .NotEmpty().WithMessage("Resposta não pode ser vazia.")
            .MaximumLength(4000);
    }
}

#endregion

#region ====================  QUERIES  ====================

public sealed record GetFeedbacksBySessionQuery(
    Guid CurrentUserId,
    bool ViewerIsAdminOrGymManager,
    Guid WorkoutSessionId)
    : IQuery<FeedbacksBySessionBundleResponse>;

public sealed record GetMyFeedbacksQuery(
    Guid CurrentStudentId,
    int Page = 1,
    int PageSize = 20,
    Guid? WorkoutSessionId = null,
    bool? OnlyUnread = null,
    FeedbackLevel? Level = null)
    : IQuery<PaginatedResponse<UnifiedFeedbackItemResponse>>;

public sealed record GetStudentFeedbacksQuery(
    Guid CurrentCoachOrAdminId,
    bool ViewerIsAdminOrGymManager,
    Guid StudentId,
    int Page = 1,
    int PageSize = 20,
    Guid? WorkoutSessionId = null,
    FeedbackLevel? Level = null)
    : IQuery<PaginatedResponse<UnifiedFeedbackItemResponse>>;

#endregion

#region ====================  HANDLERS (Create / Update / Delete)  ====================

file static class FeedbackAuthHelpers
{
    public static async Task EnsureCoachCanSendFeedbackAsync(
        IApplicationDbContext db,
        Guid coachId,
        Guid studentId,
        bool viewerIsAdminOrGymManager,
        CancellationToken ct)
    {
        if (viewerIsAdminOrGymManager) return;

        var link = await db.Set<CoachStudentLink>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.CoachId == coachId &&
                l.StudentId == studentId &&
                l.IsActive && !l.IsDeleted, ct);

        if (link == null)
            throw new DomainException("Não há vínculo ativo com este aluno.", ErrorCodes.FeedbackForbidden);
        if (!link.HasPermission(CoachPermissions.CanSendFeedback))
            throw new DomainException("Você não tem permissão para enviar feedback a este aluno.", ErrorCodes.FeedbackCannotSendNoPermission);
    }

    public static string CoachFullName(AppUser? u) =>
        u == null ? string.Empty : string.IsNullOrWhiteSpace(u.DisplayName) ? u.Email ?? string.Empty : u.DisplayName;

    public static string StudentFullName(AppUser? u) => CoachFullName(u);
}

public sealed class CreateWorkoutFeedbackCommandHandler
    : IRequestHandler<CreateWorkoutFeedbackCommand, WorkoutFeedbackResponse>
{
    private readonly IApplicationDbContext _db;
    public CreateWorkoutFeedbackCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<WorkoutFeedbackResponse> Handle(CreateWorkoutFeedbackCommand c, CancellationToken ct)
    {
        var session = await _db.Set<WorkoutSession>()
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == c.WorkoutSessionId, ct);
        if (session == null)
            throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);

        await FeedbackAuthHelpers.EnsureCoachCanSendFeedbackAsync(_db, c.CurrentCoachId, session.StudentId,
            c.ViewerIsAdminOrGymManager, ct);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == c.CurrentCoachId)
            .Select(u => u.DisplayName ?? u.Email ?? string.Empty)
            .FirstOrDefaultAsync(ct);

        var fb = new WorkoutFeedback(
            coachId: c.CurrentCoachId,
            studentId: session.StudentId,
            workoutSessionId: session.Id,
            text: c.Text,
            tone: c.Tone,
            isPublic: c.IsPublic);

        _db.Set<WorkoutFeedback>().Add(fb);

        var notif = new Notification(
            userId: session.StudentId,
            type: NotificationType.FeedbackReceived,
            title: "Novo feedback no seu treino",
            message: $"Seu professor enviou um feedback sobre sua sessão de treino.",
            referenceType: NotificationReferenceType.WorkoutSession,
            referenceId: session.Id);
        _db.Set<Notification>().Add(notif);

        await _db.SaveChangesAsync(ct);

        return new WorkoutFeedbackResponse(
            fb.Id, fb.CoachId, coachName, fb.StudentId, fb.WorkoutSessionId,
            fb.Text, fb.Tone, fb.IsPublic, fb.CreatedAt, fb.UpdatedAt, fb.ReadAt);
    }
}

public sealed class CreateExerciseFeedbackCommandHandler
    : IRequestHandler<CreateExerciseFeedbackCommand, ExerciseFeedbackResponse>
{
    private readonly IApplicationDbContext _db;
    public CreateExerciseFeedbackCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<ExerciseFeedbackResponse> Handle(CreateExerciseFeedbackCommand c, CancellationToken ct)
    {
        var wex = await _db.Set<WorkoutExercise>()
            .Include(x => x.WorkoutSession).ThenInclude(s => s!.Student)
            .Include(x => x.Exercise)
            .FirstOrDefaultAsync(x => x.Id == c.WorkoutExerciseId, ct);
        if (wex == null || wex.WorkoutSession == null)
            throw new DomainException("Exercício do treino não encontrado.", ErrorCodes.WorkoutExerciseNotFound);

        var studentId = wex.WorkoutSession.StudentId;
        await FeedbackAuthHelpers.EnsureCoachCanSendFeedbackAsync(_db, c.CurrentCoachId, studentId,
            c.ViewerIsAdminOrGymManager, ct);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == c.CurrentCoachId)
            .Select(u => u.DisplayName ?? u.Email ?? string.Empty)
            .FirstOrDefaultAsync(ct);

        var fb = new ExerciseFeedback(
            coachId: c.CurrentCoachId,
            studentId: studentId,
            workoutSessionId: wex.WorkoutSessionId,
            workoutExerciseId: wex.Id,
            text: c.Text,
            tone: c.Tone,
            isPublic: c.IsPublic);
        _db.Set<ExerciseFeedback>().Add(fb);

        var notif = new Notification(
            userId: studentId,
            type: NotificationType.FeedbackReceived,
            title: "Novo feedback em exercício",
            message: $"Seu professor comentou sobre o exercício {wex.Exercise?.Name ?? "executado"}.",
            referenceType: NotificationReferenceType.WorkoutExercise,
            referenceId: wex.Id);
        _db.Set<Notification>().Add(notif);

        await _db.SaveChangesAsync(ct);

        return new ExerciseFeedbackResponse(
            fb.Id, fb.CoachId, coachName, fb.StudentId, fb.WorkoutSessionId, fb.WorkoutExerciseId,
            wex.Exercise?.Name, fb.Text, fb.Tone, fb.IsPublic,
            fb.CreatedAt, fb.UpdatedAt, fb.ReadAt, fb.StudentResponseText, fb.StudentRespondedAt);
    }
}

public sealed class CreateSetFeedbackCommandHandler
    : IRequestHandler<CreateSetFeedbackCommand, SetFeedbackResponse>
{
    private readonly IApplicationDbContext _db;
    public CreateSetFeedbackCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<SetFeedbackResponse> Handle(CreateSetFeedbackCommand c, CancellationToken ct)
    {
        var ws = await _db.Set<WorkoutSet>()
            .Include(s => s.WorkoutExercise).ThenInclude(x => x!.WorkoutSession).ThenInclude(s2 => s2!.Student)
            .Include(s => s.WorkoutExercise).ThenInclude(x => x!.Exercise)
            .FirstOrDefaultAsync(s => s.Id == c.WorkoutSetId, ct);
        if (ws == null || ws.WorkoutExercise?.WorkoutSession == null)
            throw new DomainException("Série de treino não encontrada.", ErrorCodes.WorkoutSetNotFound);

        var studentId = ws.WorkoutExercise.WorkoutSession.StudentId;
        await FeedbackAuthHelpers.EnsureCoachCanSendFeedbackAsync(_db, c.CurrentCoachId, studentId,
            c.ViewerIsAdminOrGymManager, ct);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == c.CurrentCoachId)
            .Select(u => u.DisplayName ?? u.Email ?? string.Empty)
            .FirstOrDefaultAsync(ct);

        var fb = new SetFeedback(
            coachId: c.CurrentCoachId,
            studentId: studentId,
            workoutSessionId: ws.WorkoutExercise.WorkoutSessionId,
            workoutExerciseId: ws.WorkoutExerciseId,
            workoutSetId: ws.Id,
            text: c.Text,
            tone: c.Tone,
            isPublic: c.IsPublic,
            mediaReferenceUrl: c.MediaReferenceUrl);
        _db.Set<SetFeedback>().Add(fb);

        var notif = new Notification(
            userId: studentId,
            type: NotificationType.FeedbackReceived,
            title: "Novo feedback em série",
            message: $"Seu professor comentou sobre uma série em {ws.WorkoutExercise.Exercise?.Name ?? "seu treino"}.",
            referenceType: NotificationReferenceType.WorkoutSet,
            referenceId: ws.Id);
        _db.Set<Notification>().Add(notif);

        await _db.SaveChangesAsync(ct);

        return new SetFeedbackResponse(
            fb.Id, fb.CoachId, coachName, fb.StudentId, fb.WorkoutSessionId,
            fb.WorkoutExerciseId, fb.WorkoutSetId, ws.WorkoutExercise.Exercise?.Name,
            ws.OrderNumber, fb.Text, fb.Tone, fb.IsPublic, fb.CreatedAt, fb.UpdatedAt,
            fb.ReadAt, fb.MediaReferenceUrl);
    }
}

public sealed class UpdateFeedbackCommandHandler
    : IRequestHandler<UpdateFeedbackCommand, UnifiedFeedbackItemResponse>
{
    private readonly IApplicationDbContext _db;
    public UpdateFeedbackCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<UnifiedFeedbackItemResponse> Handle(UpdateFeedbackCommand c, CancellationToken ct)
    {
        async Task<UnifiedFeedbackItemResponse> ProjectFromAnyAsync()
        {
            var unified = await FeedbackQueryBuilders.UnifiedFeedbacks(_db)
                .Where(x => x.Id == c.FeedbackId && x.Level == c.Level)
                .FirstOrDefaultAsync(ct);
            if (unified == null)
                throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
            return unified;
        }

        switch (c.Level)
        {
            case FeedbackLevel.Session:
                var wf = await _db.Set<WorkoutFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (wf == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                if (!c.ViewerIsAdminOrGymManager && wf.CoachId != c.CurrentUserId)
                    throw new DomainException("Somente o autor ou admin pode editar.", ErrorCodes.FeedbackForbidden);
                wf.UpdateContent(c.Text, c.Tone, wf.CoachId, c.IsPublic);
                break;
            case FeedbackLevel.Exercise:
                var ef = await _db.Set<ExerciseFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (ef == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                if (!c.ViewerIsAdminOrGymManager && ef.CoachId != c.CurrentUserId)
                    throw new DomainException("Somente o autor ou admin pode editar.", ErrorCodes.FeedbackForbidden);
                ef.UpdateContent(c.Text, c.Tone, ef.CoachId, c.IsPublic);
                break;
            case FeedbackLevel.Set:
                var sf = await _db.Set<SetFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (sf == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                if (!c.ViewerIsAdminOrGymManager && sf.CoachId != c.CurrentUserId)
                    throw new DomainException("Somente o autor ou admin pode editar.", ErrorCodes.FeedbackForbidden);
                sf.UpdateContent(c.Text, c.Tone, sf.CoachId, c.IsPublic, c.MediaReferenceUrl);
                break;
            default:
                throw new DomainException("Nível de feedback inválido.", ErrorCodes.ValidationError);
        }

        await _db.SaveChangesAsync(ct);
        return await ProjectFromAnyAsync();
    }
}

public sealed class DeleteFeedbackCommandHandler : IRequestHandler<DeleteFeedbackCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteFeedbackCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<bool> Handle(DeleteFeedbackCommand c, CancellationToken ct)
    {
        switch (c.Level)
        {
            case FeedbackLevel.Session:
                var wf = await _db.Set<WorkoutFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (wf == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                if (!c.ViewerIsAdminOrGymManager && wf.CoachId != c.CurrentUserId)
                    throw new DomainException("Somente o autor ou admin pode excluir.", ErrorCodes.FeedbackForbidden);
                wf.Delete();
                break;
            case FeedbackLevel.Exercise:
                var ef = await _db.Set<ExerciseFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (ef == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                if (!c.ViewerIsAdminOrGymManager && ef.CoachId != c.CurrentUserId)
                    throw new DomainException("Somente o autor ou admin pode excluir.", ErrorCodes.FeedbackForbidden);
                ef.Delete();
                break;
            case FeedbackLevel.Set:
                var sf = await _db.Set<SetFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (sf == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                if (!c.ViewerIsAdminOrGymManager && sf.CoachId != c.CurrentUserId)
                    throw new DomainException("Somente o autor ou admin pode excluir.", ErrorCodes.FeedbackForbidden);
                sf.Delete();
                break;
            default:
                throw new DomainException("Nível de feedback inválido.", ErrorCodes.ValidationError);
        }
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class MarkFeedbackReadCommandHandler : IRequestHandler<MarkFeedbackReadCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public MarkFeedbackReadCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<bool> Handle(MarkFeedbackReadCommand c, CancellationToken ct)
    {
        switch (c.Level)
        {
            case FeedbackLevel.Session:
                var wf = await _db.Set<WorkoutFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (wf == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                wf.MarkAsRead(c.CurrentUserId);
                break;
            case FeedbackLevel.Exercise:
                var ef = await _db.Set<ExerciseFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (ef == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                ef.MarkAsRead(c.CurrentUserId);
                break;
            case FeedbackLevel.Set:
                var sf = await _db.Set<SetFeedback>().FirstOrDefaultAsync(f => f.Id == c.FeedbackId, ct);
                if (sf == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);
                sf.MarkAsRead(c.CurrentUserId);
                break;
            default:
                throw new DomainException("Nível de feedback inválido.", ErrorCodes.ValidationError);
        }
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class RespondToExerciseFeedbackCommandHandler
    : IRequestHandler<RespondToExerciseFeedbackCommand, ExerciseFeedbackResponse>
{
    private readonly IApplicationDbContext _db;
    public RespondToExerciseFeedbackCommandHandler(IApplicationDbContext db) { _db = db; }

    public async Task<ExerciseFeedbackResponse> Handle(RespondToExerciseFeedbackCommand c, CancellationToken ct)
    {
        var ef = await _db.Set<ExerciseFeedback>()
            .Include(f => f.WorkoutExercise).ThenInclude(x => x!.Exercise)
            .FirstOrDefaultAsync(f => f.Id == c.ExerciseFeedbackId, ct);
        if (ef == null) throw new DomainException("Feedback não encontrado.", ErrorCodes.FeedbackNotFound);

        ef.SetStudentResponse(c.ResponseText, c.CurrentStudentUserId);

        var coachName = await _db.Set<AppUser>()
            .Where(u => u.Id == ef.CoachId)
            .Select(u => u.DisplayName ?? u.Email ?? string.Empty)
            .FirstOrDefaultAsync(ct);

        await _db.SaveChangesAsync(ct);

        return new ExerciseFeedbackResponse(
            ef.Id, ef.CoachId, coachName, ef.StudentId, ef.WorkoutSessionId, ef.WorkoutExerciseId,
            ef.WorkoutExercise?.Exercise?.Name, ef.Text, ef.Tone, ef.IsPublic,
            ef.CreatedAt, ef.UpdatedAt, ef.ReadAt, ef.StudentResponseText, ef.StudentRespondedAt);
    }
}

#endregion

#region ====================  HANDLERS (Queries + BuildUtils)  ====================

file static class FeedbackQueryBuilders
{
    public static IQueryable<UnifiedFeedbackItemResponse> UnifiedFeedbacks(IApplicationDbContext db)
    {
        var sessions =
            from f in db.Set<WorkoutFeedback>()
            join coach in db.Set<AppUser>() on f.CoachId equals coach.Id into gj
            from coach in gj.DefaultIfEmpty()
            join stu in db.Set<AppUser>() on f.StudentId equals stu.Id into gj2
            from stu in gj2.DefaultIfEmpty()
            join ws in db.Set<WorkoutSession>() on f.WorkoutSessionId equals ws.Id into gj3
            from ws in gj3.DefaultIfEmpty()
            select new UnifiedFeedbackItemResponse
            {
                Id = f.Id,
                Level = FeedbackLevel.Session,
                CoachId = f.CoachId,
                CoachName = (coach != null ? coach.DisplayName ?? coach.Email : null),
                StudentId = f.StudentId,
                StudentName = (stu != null ? stu.DisplayName ?? stu.Email : null),
                WorkoutSessionId = f.WorkoutSessionId,
                SessionName = ws != null ? ws.Name : null,
                WorkoutExerciseId = null,
                ExerciseName = null,
                WorkoutSetId = null,
                SetOrderNumber = null,
                Text = f.Text,
                Tone = f.Tone,
                IsPublic = f.IsPublic,
                IsRead = f.ReadAt.HasValue,
                CreatedAt = f.CreatedAt,
                ReadAt = f.ReadAt,
                MediaReferenceUrl = null,
                StudentResponseText = null,
                StudentRespondedAt = null
            };

        var exercises =
            from f in db.Set<ExerciseFeedback>()
            join coach in db.Set<AppUser>() on f.CoachId equals coach.Id into gj
            from coach in gj.DefaultIfEmpty()
            join stu in db.Set<AppUser>() on f.StudentId equals stu.Id into gj2
            from stu in gj2.DefaultIfEmpty()
            join ws in db.Set<WorkoutSession>() on f.WorkoutSessionId equals ws.Id into gj3
            from ws in gj3.DefaultIfEmpty()
            join wex in db.Set<WorkoutExercise>() on f.WorkoutExerciseId equals wex.Id into gj4
            from wex in gj4.DefaultIfEmpty()
            join ex in db.Set<Domain.Exercises.Exercise>() on wex!.ExerciseId equals ex.Id into gj5
            from ex in gj5.DefaultIfEmpty()
            select new UnifiedFeedbackItemResponse
            {
                Id = f.Id,
                Level = FeedbackLevel.Exercise,
                CoachId = f.CoachId,
                CoachName = (coach != null ? coach.DisplayName ?? coach.Email : null),
                StudentId = f.StudentId,
                StudentName = (stu != null ? stu.DisplayName ?? stu.Email : null),
                WorkoutSessionId = f.WorkoutSessionId,
                SessionName = ws != null ? ws.Name : null,
                WorkoutExerciseId = f.WorkoutExerciseId,
                ExerciseName = ex != null ? ex.Name : null,
                WorkoutSetId = null,
                SetOrderNumber = null,
                Text = f.Text,
                Tone = f.Tone,
                IsPublic = f.IsPublic,
                IsRead = f.ReadAt.HasValue,
                CreatedAt = f.CreatedAt,
                ReadAt = f.ReadAt,
                MediaReferenceUrl = null,
                StudentResponseText = f.StudentResponseText,
                StudentRespondedAt = f.StudentRespondedAt
            };

        var sets =
            from f in db.Set<SetFeedback>()
            join coach in db.Set<AppUser>() on f.CoachId equals coach.Id into gj
            from coach in gj.DefaultIfEmpty()
            join stu in db.Set<AppUser>() on f.StudentId equals stu.Id into gj2
            from stu in gj2.DefaultIfEmpty()
            join ws in db.Set<WorkoutSession>() on f.WorkoutSessionId equals ws.Id into gj3
            from ws in gj3.DefaultIfEmpty()
            join wex in db.Set<WorkoutExercise>() on f.WorkoutExerciseId equals wex.Id into gj4
            from wex in gj4.DefaultIfEmpty()
            join ex in db.Set<Domain.Exercises.Exercise>() on wex!.ExerciseId equals ex.Id into gj5
            from ex in gj5.DefaultIfEmpty()
            join wset in db.Set<WorkoutSet>() on f.WorkoutSetId equals wset.Id into gj6
            from wset in gj6.DefaultIfEmpty()
            select new UnifiedFeedbackItemResponse
            {
                Id = f.Id,
                Level = FeedbackLevel.Set,
                CoachId = f.CoachId,
                CoachName = (coach != null ? coach.DisplayName ?? coach.Email : null),
                StudentId = f.StudentId,
                StudentName = (stu != null ? stu.DisplayName ?? stu.Email : null),
                WorkoutSessionId = f.WorkoutSessionId,
                SessionName = ws != null ? ws.Name : null,
                WorkoutExerciseId = f.WorkoutExerciseId,
                ExerciseName = ex != null ? ex.Name : null,
                WorkoutSetId = f.WorkoutSetId,
                SetOrderNumber = wset != null ? wset.OrderNumber : null,
                Text = f.Text,
                Tone = f.Tone,
                IsPublic = f.IsPublic,
                IsRead = f.ReadAt.HasValue,
                CreatedAt = f.CreatedAt,
                ReadAt = f.ReadAt,
                MediaReferenceUrl = f.MediaReferenceUrl,
                StudentResponseText = null,
                StudentRespondedAt = null
            };

        return sessions.Concat(exercises).Concat(sets);
    }
}

public sealed class GetFeedbacksBySessionQueryHandler
    : IRequestHandler<GetFeedbacksBySessionQuery, FeedbacksBySessionBundleResponse>
{
    private readonly IApplicationDbContext _db;
    public GetFeedbacksBySessionQueryHandler(IApplicationDbContext db) { _db = db; }

    public async Task<FeedbacksBySessionBundleResponse> Handle(GetFeedbacksBySessionQuery q, CancellationToken ct)
    {
        var session = await _db.Set<WorkoutSession>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == q.WorkoutSessionId, ct);
        if (session == null)
            throw new DomainException("Sessão de treino não encontrada.", ErrorCodes.WorkoutSessionNotFound);

        var isStudent = session.StudentId == q.CurrentUserId;
        var canCoachView = q.ViewerIsAdminOrGymManager;

        if (!isStudent && !canCoachView)
        {
            var linkOk = await _db.Set<CoachStudentLink>()
                .AsNoTracking()
                .AnyAsync(l => l.CoachId == q.CurrentUserId
                            && l.StudentId == session.StudentId
                            && l.IsActive && !l.IsDeleted
                            && (l.Permissions & CoachPermissions.CanViewWorkoutHistory) != 0, ct);
            if (!linkOk)
                throw new DomainException("Você não tem permissão para visualizar esta sessão.", ErrorCodes.FeedbackForbidden);
        }

        var wf = await _db.Set<WorkoutFeedback>()
            .Where(f => f.WorkoutSessionId == q.WorkoutSessionId)
            .OrderByDescending(f => f.CreatedAt)
            .Join(_db.Set<AppUser>(), f => f.CoachId, u => u.Id, (f, u) => new { f, u })
            .Select(x => new WorkoutFeedbackResponse(
                x.f.Id, x.f.CoachId, (x.u.DisplayName ?? x.u.Email ?? string.Empty), x.f.StudentId, x.f.WorkoutSessionId,
                x.f.Text, x.f.Tone, x.f.IsPublic, x.f.CreatedAt, x.f.UpdatedAt, x.f.ReadAt))
            .ToListAsync(ct);

        var ef = await (
            from f in _db.Set<ExerciseFeedback>()
            where f.WorkoutSessionId == q.WorkoutSessionId
            join coach in _db.Set<AppUser>() on f.CoachId equals coach.Id
            join wex in _db.Set<WorkoutExercise>() on f.WorkoutExerciseId equals wex.Id
            join ex in _db.Set<Domain.Exercises.Exercise>() on wex.ExerciseId equals ex.Id into ejoin
            from ex in ejoin.DefaultIfEmpty()
            orderby f.CreatedAt descending
            select new ExerciseFeedbackResponse(
                f.Id, f.CoachId, coach.DisplayName ?? coach.Email ?? string.Empty,
                f.StudentId, f.WorkoutSessionId, f.WorkoutExerciseId, ex != null ? ex.Name : null,
                f.Text, f.Tone, f.IsPublic, f.CreatedAt, f.UpdatedAt, f.ReadAt,
                f.StudentResponseText, f.StudentRespondedAt)
            ).ToListAsync(ct);

        var sf = await (
            from f in _db.Set<SetFeedback>()
            where f.WorkoutSessionId == q.WorkoutSessionId
            join coach in _db.Set<AppUser>() on f.CoachId equals coach.Id
            join wex in _db.Set<WorkoutExercise>() on f.WorkoutExerciseId equals wex.Id
            join ex in _db.Set<Domain.Exercises.Exercise>() on wex.ExerciseId equals ex.Id into ejoin
            from ex in ejoin.DefaultIfEmpty()
            join wset in _db.Set<WorkoutSet>() on f.WorkoutSetId equals wset.Id into sjoin
            from wset in sjoin.DefaultIfEmpty()
            orderby f.CreatedAt descending
            select new SetFeedbackResponse(
                f.Id, f.CoachId, coach.DisplayName ?? coach.Email ?? string.Empty,
                f.StudentId, f.WorkoutSessionId, f.WorkoutExerciseId, f.WorkoutSetId,
                ex != null ? ex.Name : null, wset != null ? wset.OrderNumber : 0,
                f.Text, f.Tone, f.IsPublic, f.CreatedAt, f.UpdatedAt, f.ReadAt, f.MediaReferenceUrl)
            ).ToListAsync(ct);

        return new FeedbacksBySessionBundleResponse(q.WorkoutSessionId, wf, ef, sf);
    }
}

public sealed class GetMyFeedbacksQueryHandler
    : IRequestHandler<GetMyFeedbacksQuery, PaginatedResponse<UnifiedFeedbackItemResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetMyFeedbacksQueryHandler(IApplicationDbContext db) { _db = db; }

    public async Task<PaginatedResponse<UnifiedFeedbackItemResponse>> Handle(GetMyFeedbacksQuery q, CancellationToken ct)
    {
        var query = FeedbackQueryBuilders.UnifiedFeedbacks(_db)
            .Where(x => x.StudentId == q.CurrentStudentId);

        if (q.WorkoutSessionId.HasValue)
            query = query.Where(x => x.WorkoutSessionId == q.WorkoutSessionId.Value);
        if (q.OnlyUnread == true)
            query = query.Where(x => !x.IsRead);
        if (q.Level.HasValue)
            query = query.Where(x => x.Level == q.Level.Value);

        query = query.OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return new PaginatedResponse<UnifiedFeedbackItemResponse>(items, q.Page, q.PageSize, total);
    }
}

public sealed class GetStudentFeedbacksQueryHandler
    : IRequestHandler<GetStudentFeedbacksQuery, PaginatedResponse<UnifiedFeedbackItemResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetStudentFeedbacksQueryHandler(IApplicationDbContext db) { _db = db; }

    public async Task<PaginatedResponse<UnifiedFeedbackItemResponse>> Handle(GetStudentFeedbacksQuery q, CancellationToken ct)
    {
        if (!q.ViewerIsAdminOrGymManager)
        {
            var linkOk = await _db.Set<CoachStudentLink>()
                .AsNoTracking()
                .AnyAsync(l => l.CoachId == q.CurrentCoachOrAdminId
                            && l.StudentId == q.StudentId
                            && l.IsActive && !l.IsDeleted
                            && (l.Permissions & CoachPermissions.CanViewWorkoutHistory) != 0, ct);
            if (!linkOk)
                throw new DomainException("Você não tem permissão para visualizar feedbacks deste aluno.", ErrorCodes.FeedbackForbidden);
        }

        var query = FeedbackQueryBuilders.UnifiedFeedbacks(_db)
            .Where(x => x.StudentId == q.StudentId);

        if (!q.ViewerIsAdminOrGymManager)
            query = query.Where(x => x.CoachId == q.CurrentCoachOrAdminId);

        if (q.WorkoutSessionId.HasValue)
            query = query.Where(x => x.WorkoutSessionId == q.WorkoutSessionId.Value);
        if (q.Level.HasValue)
            query = query.Where(x => x.Level == q.Level.Value);

        query = query.OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return new PaginatedResponse<UnifiedFeedbackItemResponse>(items, q.Page, q.PageSize, total);
    }
}

#endregion
