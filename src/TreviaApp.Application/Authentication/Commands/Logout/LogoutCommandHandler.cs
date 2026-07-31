namespace TreviaApp.Application.Authentication.Commands.Logout;
using MediatR;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Shared.Constants;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly ICurrentUserService _current;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IMediator _mediator;

    public LogoutCommandHandler(ICurrentUserService current, ILogger<LogoutCommandHandler> logger, IMediator mediator)
    {
        _current = current;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!_current.UserId.HasValue)
            throw new DomainException("Não autenticado.", ErrorCodes.Unauthorized);

        await _mediator.Send(new RevokeAllRefreshTokens.RevokeAllRefreshTokensCommand(_current.UserId, "Logout"), cancellationToken);
        _logger.LogInformation("UserLoggedOut UserId={UserId}", _current.UserId.Value);
    }
}
