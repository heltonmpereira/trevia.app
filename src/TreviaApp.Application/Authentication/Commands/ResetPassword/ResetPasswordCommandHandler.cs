namespace TreviaApp.Application.Authentication.Commands.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(UserManager<AppUser> userManager, ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            throw new DomainException("Dados inválidos.", ErrorCodes.ValidationError);

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => (object?)e.Description);
            throw new DomainException("Falha ao redefinir senha.", ErrorCodes.ValidationError, errors);
        }

        _logger.LogInformation("PasswordReset UserId={UserId}", user.Id);
    }
}
