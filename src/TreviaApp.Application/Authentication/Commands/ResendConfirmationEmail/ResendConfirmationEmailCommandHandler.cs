namespace TreviaApp.Application.Authentication.Commands.ResendConfirmationEmail;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TreviaApp.Application.Email;
using TreviaApp.Application.Security;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class ResendConfirmationEmailCommandHandler : ICommandHandler<ResendConfirmationEmailCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _current;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger;

    public ResendConfirmationEmailCommandHandler(UserManager<AppUser> userManager, ICurrentUserService current, IEmailSender emailSender, ILogger<ResendConfirmationEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _current = current;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        AppUser? user;
        if (request.UserId.HasValue)
            user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
        else if (!string.IsNullOrWhiteSpace(request.Email))
            user = await _userManager.FindByEmailAsync(request.Email);
        else if (_current.UserId.HasValue)
            user = await _userManager.FindByIdAsync(_current.UserId.Value.ToString());
        else
            throw new DomainException("Nenhum usuário identificado.", ErrorCodes.ValidationError);

        if (user is null) return;
        if (user.EmailConfirmed) return;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = $"http://localhost:5005/auth/confirm-email?userId={Uri.EscapeDataString(user.Id.ToString())}&token={Uri.EscapeDataString(token)}";

        try
        {
            await _emailSender.SendConfirmationEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}".Trim(), link, cancellationToken);
            _logger.LogInformation("ConfirmationEmailResent UserId={UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ConfirmationEmailResendFailed UserId={UserId}", user.Id);
        }
    }
}
