namespace TreviaApp.Application.Authentication.Commands.ConfirmEmail;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(UserManager<AppUser> userManager, ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            throw new DomainException("Usuário não encontrado.", ErrorCodes.NotFound);

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => (object?)e.Description);
            throw new DomainException("Token de confirmação inválido ou expirado.", ErrorCodes.ValidationError, errors);
        }

        _logger.LogInformation("EmailConfirmed UserId={UserId}", user.Id);
    }
}
