namespace TreviaApp.Contracts.Common;

public class FileResult
{
    public FileResult() { }

    public FileResult(string fileName, string contentType, long sizeBytes, string temporaryUrl, DateTimeOffset? expiresAt)
    {
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        TemporaryUrl = temporaryUrl;
        ExpiresAt = expiresAt;
    }

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string TemporaryUrl { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; set; }
}
