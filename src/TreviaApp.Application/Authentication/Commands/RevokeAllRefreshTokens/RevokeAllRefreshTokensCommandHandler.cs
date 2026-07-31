namespace TreviaApp.Application.Authentication.Commands.RevokeAllRefreshTokens;
using MediatR;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Shared.Constants;

public class RevokeAllRefreshTokensCommandHandler : ICommandHandler<RevokeAllRefreshTokensCommand>
{
    private readonly IRefreshTokenStore _store;
    private readonly ICurrentUserService _current;
    private readonly ILogger<RevokeAllRefreshTokensCommandHandler> _logger;

    public RevokeAllRefreshTokensCommandHandler(IRefreshTokenStore store, ICurrentUserService current, ILogger<RevokeAllRefreshTokensCommandHandler> logger)
    {
        _store = store;
        _current = current;
        _logger = logger;
    }

    public async Task Handle(RevokeAllRefreshTokensCommand request, CancellationToken cancellationToken)
    {
        var target = request.TargetUserId ?? _current.UserId;
        if (!target.HasValue)
            throw new DomainException("Usuário não autenticado.", ErrorCodes.Unauthorized);

        await _store.RevokeAllForUserAsync(target.Value, request.Reason, cancellationToken);
        _logger.LogInformation("AllTokensRevoked UserId={UserId} Reason={Reason}", target.Value, request.Reason);
    }
}
