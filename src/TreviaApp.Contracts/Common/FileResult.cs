namespace TreviaApp.Contracts.Common;

/// <summary>
/// Represents the FileResult contract.
/// </summary>
public class FileResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="FileResult"/>.
    /// </summary>
    public FileResult() { }

    /// <summary>
    /// Initializes a new instance of <see cref="FileResult"/>.
    /// </summary>
    public FileResult(string fileName, string contentType, long sizeBytes, string temporaryUrl, DateTimeOffset? expiresAt)
    {
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        TemporaryUrl = temporaryUrl;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Gets or sets File Name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Content Type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Size Bytes.
    /// </summary>
    public long SizeBytes { get; set; }
    /// <summary>
    /// Gets or sets Temporary Url.
    /// </summary>
    public string TemporaryUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Expires At.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
