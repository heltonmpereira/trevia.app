namespace TreviaApp.Application.Authentication.Commands.RevokeRefreshToken;
using MediatR;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Shared.Constants;

public class RevokeRefreshTokenCommandHandler : ICommandHandler<RevokeRefreshTokenCommand>
{
    private readonly IRefreshTokenStore _store;
    private readonly ICurrentUserService _current;
    private readonly ILogger<RevokeRefreshTokenCommandHandler> _logger;

    public RevokeRefreshTokenCommandHandler(IRefreshTokenStore store, ICurrentUserService current, ILogger<RevokeRefreshTokenCommandHandler> logger)
    {
        _store = store;
        _current = current;
        _logger = logger;
    }

    public async Task Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenId = request.RefreshToken.IndexOf('.') is var i && i > 0 ? request.RefreshToken.Substring(0, i) : request.RefreshToken;
        var rec = await _store.GetByTokenIdAsync(tokenId, cancellationToken);
        if (rec is null)
            throw new DomainException("Token não encontrado.", ErrorCodes.NotFound);

        if (_current.UserId.HasValue && rec.UserId != _current.UserId.Value)
            throw new DomainException("Este token não pertence a você.", ErrorCodes.Forbidden);

        await _store.RevokeByTokenIdAsync(tokenId, request.Reason, cancellationToken);
        _logger.LogInformation("TokenRevoked TokenId={TokenId} UserId={UserId} Reason={Reason}", tokenId, rec.UserId, request.Reason);
    }
}
