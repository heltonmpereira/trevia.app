namespace TreviaApp.Infrastructure.Email;

public interface IEmailSender
{
    Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationLink, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, CancellationToken ct = default);
}
