namespace TreviaApp.Application.Authentication.Commands.ChangePassword;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _current;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(UserManager<AppUser> userManager, ICurrentUserService current, ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _current = current;
        _logger = logger;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_current.UserId.HasValue)
            throw new DomainException("Usuário não autenticado.", ErrorCodes.Unauthorized);

        var user = await _userManager.FindByIdAsync(_current.UserId.Value.ToString());
        if (user is null)
            throw new DomainException("Usuário não encontrado.", ErrorCodes.NotFound);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => (object?)e.Description);
            throw new DomainException("Falha ao alterar senha.", ErrorCodes.ValidationError, errors);
        }

        _logger.LogInformation("PasswordChanged UserId={UserId}", user.Id);
    }
}
