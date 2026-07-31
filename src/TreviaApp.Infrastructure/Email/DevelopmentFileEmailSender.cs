namespace TreviaApp.Infrastructure.Email;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using TreviaApp.Application.Email;

public class DevelopmentFileEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<DevelopmentFileEmailSender> _logger;

    public DevelopmentFileEmailSender(IOptions<EmailOptions> options, ILogger<DevelopmentFileEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationLink, CancellationToken ct = default)
    {
        var subject = "Confirme seu e-mail — TreviaApp";
        var body = $"Olá {toName},\n\nPor favor clique no link abaixo para confirmar seu e-mail:\n{confirmationLink}\n\nSe você não solicitou, ignore.\n\nEquipe TreviaApp";
        var html = $"<h1>Confirme seu e-mail</h1><p>Olá {toName},</p><p>Por favor <a href=\"{confirmationLink}\">clique aqui</a> para confirmar seu e-mail.</p>";
        return SendGenericEmailAsync(toEmail, toName, subject, body, html, ct);
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, CancellationToken ct = default)
    {
        var subject = "Redefinição de senha — TreviaApp";
        var body = $"Olá {toName},\n\nClique no link abaixo para redefinir sua senha:\n{resetLink}\n\nSe você não solicitou, ignore.\n\nEquipe TreviaApp";
        var html = $"<h1>Redefina sua senha</h1><p>Olá {toName},</p><p><a href=\"{resetLink}\">Clique aqui</a> para redefinir sua senha.</p>";
        return SendGenericEmailAsync(toEmail, toName, subject, body, html, ct);
    }

    public async Task SendGenericEmailAsync(string toEmail, string toName, string subject, string plainTextBody, string? htmlBody = null, CancellationToken ct = default)
    {
        var outDir = string.IsNullOrWhiteSpace(_options.OutputFolder)
            ? Path.Combine(Path.GetTempPath(), "treviaapp-emails-out")
            : _options.OutputFolder;

        Directory.CreateDirectory(outDir);

        var filename = $"email-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.eml";
        var path = Path.Combine(outDir, filename);

        var eml = new StringBuilder();
        eml.AppendLine("From: \"TreviaApp\" <noreply@trevia.app>");
        eml.AppendLine($"To: \"{toName}\" <{toEmail}>");
        eml.AppendLine($"Subject: {subject}");
        eml.AppendLine($"Date: {DateTimeOffset.UtcNow:R}");
        eml.AppendLine("MIME-Version: 1.0");

        if (htmlBody != null)
        {
            eml.AppendLine("Content-Type: multipart/alternative; boundary=\"boundary-trevia\"");
            eml.AppendLine();
            eml.AppendLine("--boundary-trevia");
            eml.AppendLine("Content-Type: text/plain; charset=utf-8");
            eml.AppendLine("Content-Transfer-Encoding: base64");
            eml.AppendLine();
            eml.AppendLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(plainTextBody)));
            eml.AppendLine("--boundary-trevia");
            eml.AppendLine("Content-Type: text/html; charset=utf-8");
            eml.AppendLine("Content-Transfer-Encoding: base64");
            eml.AppendLine();
            eml.AppendLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(htmlBody)));
            eml.AppendLine("--boundary-trevia--");
        }
        else
        {
            eml.AppendLine("Content-Type: text/plain; charset=utf-8");
            eml.AppendLine();
            eml.Append(plainTextBody);
        }

        await File.WriteAllTextAsync(path, eml.ToString(), Encoding.UTF8, ct);
        _logger.LogInformation("E-mail salvo em arquivo (Development). Destinatário: {To}. Assunto: {Subject}. Arquivo: {Path}", toEmail, subject, path);
    }
}
