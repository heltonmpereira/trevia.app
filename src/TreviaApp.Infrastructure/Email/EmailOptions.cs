namespace TreviaApp.Infrastructure.Email;

public class EmailOptions
{
    public string Provider { get; set; } = "DevelopmentFile";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@trevia.app";
    public string FromName { get; set; } = "TreviaApp";
    public string OutputFolder { get; set; } = string.Empty;
}
