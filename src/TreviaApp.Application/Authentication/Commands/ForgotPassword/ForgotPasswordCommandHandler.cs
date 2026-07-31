namespace TreviaApp.Application.Authentication.Commands.ForgotPassword;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TreviaApp.Application.Email;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(UserManager<AppUser> userManager, IEmailSender emailSender, ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsDeleted)
        {
            _logger.LogInformation("PasswordResetRequested Email={Email} Result=NoUserFound", request.Email);
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedUserId = Uri.EscapeDataString(user.Id.ToString());
        var encodedToken = Uri.EscapeDataString(token);
        var resetLink = $"http://localhost:5005/auth/reset-password?userId={encodedUserId}&token={encodedToken}";

        try
        {
            await _emailSender.SendPasswordResetEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}".Trim(), resetLink, cancellationToken);
            _logger.LogInformation("PasswordResetEmailSent UserId={UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PasswordResetEmailFailed UserId={UserId}", user.Id);
        }
    }
}
