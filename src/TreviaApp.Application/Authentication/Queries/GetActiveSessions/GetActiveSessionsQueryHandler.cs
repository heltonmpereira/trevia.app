namespace TreviaApp.Application.Authentication.Queries.GetActiveSessions;
using MediatR;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Shared.Constants;

public class GetActiveSessionsQueryHandler : IQueryHandler<GetActiveSessionsQuery, UserSessionsResponse>
{
    private readonly IRefreshTokenStore _store;
    private readonly ICurrentUserService _current;

    public GetActiveSessionsQueryHandler(IRefreshTokenStore store, ICurrentUserService current)
    {
        _store = store;
        _current = current;
    }

    public async Task<UserSessionsResponse> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        if (!_current.UserId.HasValue)
            throw new DomainException("Não autenticado.", ErrorCodes.Unauthorized);

        var active = await _store.GetActiveForUserAsync(_current.UserId.Value, cancellationToken);
        var currentId = "TODO: detectar sessão atual via refresh";
        var items = active.Select(r => new UserSessionItem(
            SessionId: r.TokenId,
            Device: r.DeviceInfo ?? "Desconhecido",
            IpAddress: r.IpAddress ?? "—",
            StartedAt: r.CreatedAt,
            IsCurrent: r.TokenId == currentId
        )).ToList();

        return new UserSessionsResponse(items);
    }
}
