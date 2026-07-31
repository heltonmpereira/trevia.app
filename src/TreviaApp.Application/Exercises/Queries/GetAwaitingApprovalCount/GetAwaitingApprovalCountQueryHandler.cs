namespace TreviaApp.Application.Exercises.Queries.GetAwaitingApprovalCount;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Exercises;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;

public sealed class GetAwaitingApprovalCountQueryHandler : IQueryHandler<GetAwaitingApprovalCountQuery, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetAwaitingApprovalCountQueryHandler> _logger;

    public GetAwaitingApprovalCountQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<GetAwaitingApprovalCountQueryHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<int> Handle(GetAwaitingApprovalCountQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(AppRoles.Administrator))
            throw new DomainException("Acesso restrito a Administradores.", ErrorCodes.Forbidden);

        var count = await _db.Set<Exercise>()
            .AsNoTracking()
            .CountAsync(e => e.Status == ExerciseStatus.AwaitingApproval, cancellationToken);

        return count;
    }
}
